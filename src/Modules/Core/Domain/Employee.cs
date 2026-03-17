using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Core.Domain;

public sealed class Employee : AggregateRoot<EmployeeId>
{
    public Personnummer Personnummer { get; private set; } = null!;
    public string Fornamn { get; private set; } = string.Empty;
    public string Efternamn { get; private set; } = string.Empty;
    public string? MellanNamn { get; private set; }
    public string FulltNamn => MellanNamn is null ? $"{Fornamn} {Efternamn}" : $"{Fornamn} {MellanNamn} {Efternamn}";

    // Kontaktuppgifter
    public string? Epost { get; private set; }
    public string? Telefon { get; private set; }
    public Address? Adress { get; private set; }

    // Bankuppgifter (krypteras i databas)
    public string? Clearingnummer { get; private set; }
    public string? Kontonummer { get; private set; }

    // Skatteuppgifter
    public int? Skattetabell { get; private set; }    // 30-36
    public int? Skattekolumn { get; private set; }    // 1-6
    public string? Kommun { get; private set; }       // Kommun för skattesats
    public decimal? KommunalSkattesats { get; private set; }
    public bool HarKyrkoavgift { get; private set; }
    public decimal? Kyrkoavgiftssats { get; private set; }
    public bool HarJamkning { get; private set; }
    public Money? JamkningBelopp { get; private set; }

    // Anställningar
    private readonly List<Employment> _anstallningar = [];
    public IReadOnlyList<Employment> Anstallningar => _anstallningar.AsReadOnly();

    private Employee() { } // EF Core

    public static Employee Skapa(
        Personnummer personnummer,
        string fornamn,
        string efternamn,
        string? mellanNamn = null)
    {
        var employee = new Employee
        {
            Id = EmployeeId.New(),
            Personnummer = personnummer,
            Fornamn = fornamn,
            Efternamn = efternamn,
            MellanNamn = mellanNamn
        };

        employee.RaiseDomainEvent(new EmployeeCreatedEvent(employee.Id, personnummer.ToString()));
        return employee;
    }

    public void UppdateraKontaktuppgifter(string? epost, string? telefon, Address? adress)
    {
        Epost = epost;
        Telefon = telefon;
        Adress = adress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UppdateraBankuppgifter(string clearingnummer, string kontonummer)
    {
        Clearingnummer = clearingnummer;
        Kontonummer = kontonummer;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UppdateraSkatteuppgifter(
        int skattetabell, int skattekolumn, string kommun,
        decimal kommunalSkattesats, bool harKyrkoavgift, decimal? kyrkoavgiftssats)
    {
        if (skattetabell < 29 || skattetabell > 40)
            throw new ArgumentException("Skattetabell måste vara 29-40");
        if (skattekolumn < 1 || skattekolumn > 6)
            throw new ArgumentException("Skattekolumn måste vara 1-6");

        Skattetabell = skattetabell;
        Skattekolumn = skattekolumn;
        Kommun = kommun;
        KommunalSkattesats = kommunalSkattesats;
        HarKyrkoavgift = harKyrkoavgift;
        Kyrkoavgiftssats = kyrkoavgiftssats;
        UpdatedAt = DateTime.UtcNow;
    }

    public Employment LaggTillAnstallning(
        OrganizationId enhet,
        EmploymentType anstallningsform,
        CollectiveAgreementType kollektivavtal,
        Money manadslon,
        Percentage sysselsattningsgrad,
        DateOnly startdatum,
        DateOnly? slutdatum = null,
        string? bestaKod = null,
        string? aidKod = null)
    {
        var employment = Employment.Skapa(
            Id, enhet, anstallningsform, kollektivavtal,
            manadslon, sysselsattningsgrad, startdatum, slutdatum,
            bestaKod, aidKod);

        _anstallningar.Add(employment);
        RaiseDomainEvent(new EmploymentCreatedEvent(employment.Id, Id, anstallningsform));
        return employment;
    }

    public Employment? AktivAnstallning(DateOnly datum) =>
        _anstallningar.FirstOrDefault(a => a.Giltighetsperiod.IsActiveOn(datum));

    public IReadOnlyList<Employment> AktivaAnstallningar(DateOnly datum) =>
        _anstallningar.Where(a => a.Giltighetsperiod.IsActiveOn(datum)).ToList();
}

public record Address(
    string Gatuadress,
    string Postnummer,
    string Ort,
    string Land = "Sverige");
