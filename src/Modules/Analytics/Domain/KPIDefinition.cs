namespace RegionHR.Analytics.Domain;

public class KPIDefinition
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Kategori { get; private set; } = ""; // Workforce/Turnover/Absence/Compensation/Competence/Recruitment/Engagement/Compliance
    public string BerakningsFormel { get; private set; } = "";
    public string Enhet { get; private set; } = ""; // percent/count/currency/days
    public string Riktning { get; private set; } = ""; // HigherIsBetter/LowerIsBetter
    public decimal GronTroskel { get; private set; }
    public decimal GulTroskel { get; private set; }
    public decimal RodTroskel { get; private set; }
    public bool ArAktiv { get; private set; }

    private KPIDefinition() { }

    public static KPIDefinition Skapa(
        string namn, string kategori, string berakningsFormel,
        string enhet, string riktning,
        decimal gronTroskel, decimal gulTroskel, decimal rodTroskel,
        bool arAktiv = true)
    {
        return new KPIDefinition
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Kategori = kategori,
            BerakningsFormel = berakningsFormel,
            Enhet = enhet,
            Riktning = riktning,
            GronTroskel = gronTroskel,
            GulTroskel = gulTroskel,
            RodTroskel = rodTroskel,
            ArAktiv = arAktiv
        };
    }

    public void UppdateraTrosklar(decimal gron, decimal gul, decimal rod)
    {
        GronTroskel = gron;
        GulTroskel = gul;
        RodTroskel = rod;
    }

    public void ToggleAktiv() { ArAktiv = !ArAktiv; }
}
