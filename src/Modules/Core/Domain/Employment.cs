using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Core.Domain;

public sealed class Employment : Entity<EmploymentId>
{
    public EmployeeId AnstallId { get; private set; }
    public OrganizationId EnhetId { get; private set; }
    public EmploymentType Anstallningsform { get; private set; }
    public CollectiveAgreementType Kollektivavtal { get; private set; }
    public Money Manadslon { get; private set; }
    public Percentage Sysselsattningsgrad { get; private set; }
    public DateRange Giltighetsperiod { get; private set; } = null!;

    // BESTA/AID-koder för statistik
    public string? BESTAKod { get; private set; }
    public string? AIDKod { get; private set; }

    // Befattning
    public string? Befattningstitel { get; private set; }

    // Kollektivavtals-referens (DB-backed)
    public CollectiveAgreementId? AvtalsId { get; private set; }

    // LAS-relevant
    public bool ArTillsvidareanstallning => Anstallningsform == EmploymentType.Tillsvidare;
    public bool ArTidsbegransad => Anstallningsform is EmploymentType.Vikariat or EmploymentType.SAVA or EmploymentType.Sasongsanstallning;

    private Employment() { } // EF Core

    internal static Employment Skapa(
        EmployeeId anstallId,
        OrganizationId enhet,
        EmploymentType anstallningsform,
        CollectiveAgreementType kollektivavtal,
        Money manadslon,
        Percentage sysselsattningsgrad,
        DateOnly startdatum,
        DateOnly? slutdatum,
        string? bestaKod,
        string? aidKod)
    {
        return new Employment
        {
            Id = EmploymentId.New(),
            AnstallId = anstallId,
            EnhetId = enhet,
            Anstallningsform = anstallningsform,
            Kollektivavtal = kollektivavtal,
            Manadslon = manadslon,
            Sysselsattningsgrad = sysselsattningsgrad,
            Giltighetsperiod = new DateRange(startdatum, slutdatum),
            BESTAKod = bestaKod,
            AIDKod = aidKod
        };
    }

    public void AndraLon(Money nyLon, string andradAv)
    {
        if (nyLon.Amount <= 0)
            throw new ArgumentException("Lön måste vara positiv");
        Manadslon = nyLon;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = andradAv;
    }

    public void SattBefattning(string befattningstitel)
    {
        Befattningstitel = befattningstitel;
    }

    public void AndraSysselsattningsgrad(Percentage nyGrad)
    {
        Sysselsattningsgrad = nyGrad;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AvslutaAnstallning(DateOnly slutdatum)
    {
        Giltighetsperiod = new DateRange(Giltighetsperiod.Start, slutdatum);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SattKollektivavtal(CollectiveAgreementId avtalsId)
    {
        AvtalsId = avtalsId;
        UpdatedAt = DateTime.UtcNow;
    }

    public Money BeraknaDaglon() => Manadslon / 21m; // Genomsnittliga arbetsdagar/månad

    /// <summary>Beräkna timlön baserat på heltidstimmar per vecka (vanligen 38.25 för AB)</summary>
    public Money BeraknaTimlon(decimal veckoarbetstid = 38.25m)
    {
        // TODO: Lookup weekly hours from collective agreement instead of hardcoded 38.25
        var timmarPerManad = veckoarbetstid * 52m / 12m;
        return Manadslon / timmarPerManad;
    }
}
