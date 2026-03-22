using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>
/// Kollektivavtal — aggregatrot som samlar alla avtalsvillkor.
/// Refereras av Employment och OrganizationUnit via CollectiveAgreementId.
/// </summary>
public sealed class CollectiveAgreement : AggregateRoot<CollectiveAgreementId>
{
    public string Namn { get; private set; } = string.Empty;
    public string Parter { get; private set; } = string.Empty;
    public DateOnly GiltigFran { get; private set; }
    public DateOnly? GiltigTill { get; private set; }
    public IndustrySector Bransch { get; private set; }
    public AgreementStatus Status { get; private set; }

    /// <summary>Heltidstimmar per vecka (default 38.25 för AB). Används för timlönsberäkning.</summary>
    public decimal VeckotimmarHeltid { get; private set; } = 38.25m;

    // Sub-entity collections
    private readonly List<AgreementOBRate> _obSatser = [];
    public IReadOnlyList<AgreementOBRate> OBSatser => _obSatser.AsReadOnly();

    private readonly List<AgreementOvertimeRule> _overtidsRegler = [];
    public IReadOnlyList<AgreementOvertimeRule> OvertidsRegler => _overtidsRegler.AsReadOnly();

    private readonly List<AgreementVacationRule> _semesterRegler = [];
    public IReadOnlyList<AgreementVacationRule> SemesterRegler => _semesterRegler.AsReadOnly();

    private readonly List<AgreementPensionRule> _pensionsRegler = [];
    public IReadOnlyList<AgreementPensionRule> PensionsRegler => _pensionsRegler.AsReadOnly();

    private readonly List<AgreementRestRule> _viloRegler = [];
    public IReadOnlyList<AgreementRestRule> ViloRegler => _viloRegler.AsReadOnly();

    private readonly List<AgreementWorkingHours> _arbetstidsRegler = [];
    public IReadOnlyList<AgreementWorkingHours> ArbetstidsRegler => _arbetstidsRegler.AsReadOnly();

    private readonly List<AgreementNoticePeriod> _uppsagningsRegler = [];
    public IReadOnlyList<AgreementNoticePeriod> UppságningsRegler => _uppsagningsRegler.AsReadOnly();

    private readonly List<AgreementInsurancePackage> _forsakringsRegler = [];
    public IReadOnlyList<AgreementInsurancePackage> ForsakringsRegler => _forsakringsRegler.AsReadOnly();

    private readonly List<AgreementSalaryStructure> _lonestrukturRegler = [];
    public IReadOnlyList<AgreementSalaryStructure> LonestrukturRegler => _lonestrukturRegler.AsReadOnly();

    private readonly List<PrivateCompensationPlan> _privatErsattningsPlaner = [];
    public IReadOnlyList<PrivateCompensationPlan> PrivatErsattningsPlaner => _privatErsattningsPlaner.AsReadOnly();

    private CollectiveAgreement() { } // EF Core

    public static CollectiveAgreement Skapa(
        string namn,
        string parter,
        DateOnly giltigFran,
        IndustrySector bransch,
        decimal veckotimmarHeltid = 38.25m)
    {
        return new CollectiveAgreement
        {
            Id = CollectiveAgreementId.New(),
            Namn = namn,
            Parter = parter,
            GiltigFran = giltigFran,
            GiltigTill = null,
            Bransch = bransch,
            Status = AgreementStatus.Aktivt,
            VeckotimmarHeltid = veckotimmarHeltid
        };
    }

    public AgreementOBRate LaggTillOBSats(OBCategory tidstyp, decimal belopp, DateOnly giltigFran, DateOnly? giltigTill = null)
    {
        var sats = AgreementOBRate.Skapa(Id, tidstyp, belopp, giltigFran, giltigTill);
        _obSatser.Add(sats);
        return sats;
    }

    public AgreementOvertimeRule LaggTillOvertidsRegel(decimal troskel, decimal multiplikator, decimal maxPerAr)
    {
        var regel = AgreementOvertimeRule.Skapa(Id, troskel, multiplikator, maxPerAr);
        _overtidsRegler.Add(regel);
        return regel;
    }

    public AgreementVacationRule LaggTillSemesterRegel(int basDagar, int extraDagarVid40, int extraDagarVid50)
    {
        var regel = AgreementVacationRule.Skapa(Id, basDagar, extraDagarVid40, extraDagarVid50);
        _semesterRegler.Add(regel);
        return regel;
    }

    public AgreementPensionRule LaggTillPensionsRegel(
        PensionType pensionsTyp, decimal satsUnderTak, decimal satsOverTak, decimal tak, string berakningsModell)
    {
        var regel = AgreementPensionRule.Skapa(Id, pensionsTyp, satsUnderTak, satsOverTak, tak, berakningsModell);
        _pensionsRegler.Add(regel);
        return regel;
    }

    public AgreementRestRule LaggTillViloRegel(decimal minDygnsvila, decimal minVeckovila, decimal rastPerPass)
    {
        var regel = AgreementRestRule.Skapa(Id, minDygnsvila, minVeckovila, rastPerPass);
        _viloRegler.Add(regel);
        return regel;
    }

    public AgreementWorkingHours LaggTillArbetstidsRegel(decimal normalTimmarPerVecka, string flexRegler)
    {
        var regel = AgreementWorkingHours.Skapa(Id, normalTimmarPerVecka, flexRegler);
        _arbetstidsRegler.Add(regel);
        return regel;
    }

    public AgreementNoticePeriod LaggTillUppságningsRegel(int anstallningstidManader, int uppsagningstidManader)
    {
        var regel = AgreementNoticePeriod.Skapa(Id, anstallningstidManader, uppsagningstidManader);
        _uppsagningsRegler.Add(regel);
        return regel;
    }

    public AgreementInsurancePackage LaggTillForsakringspaket(string tgl, string ags, string tfa, string afa, string psa)
    {
        var paket = AgreementInsurancePackage.Skapa(Id, tgl, ags, tfa, afa, psa);
        _forsakringsRegler.Add(paket);
        return paket;
    }

    public AgreementSalaryStructure LaggTillLonestruktur(string minLonPerKategori, string loneSteg)
    {
        var struktur = AgreementSalaryStructure.Skapa(Id, minLonPerKategori, loneSteg);
        _lonestrukturRegler.Add(struktur);
        return struktur;
    }

    public PrivateCompensationPlan LaggTillPrivatErsattningsPlan(string bonus, string provision, string aktier, string tjanstebil)
    {
        var plan = PrivateCompensationPlan.Skapa(Id, bonus, provision, aktier, tjanstebil);
        _privatErsattningsPlaner.Add(plan);
        return plan;
    }

    /// <summary>Hämta gällande OB-sats för en given tidstyp och datum</summary>
    public decimal HamtaOBSats(OBCategory tidstyp, DateOnly datum)
    {
        var sats = _obSatser
            .Where(s => s.Tidstyp == tidstyp && s.GiltigFran <= datum && (s.GiltigTill == null || s.GiltigTill >= datum))
            .OrderByDescending(s => s.GiltigFran)
            .FirstOrDefault();

        return sats?.Belopp ?? 0m;
    }
}
