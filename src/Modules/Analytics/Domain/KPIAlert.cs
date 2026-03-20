namespace RegionHR.Analytics.Domain;

public class KPIAlert
{
    public Guid Id { get; private set; }
    public Guid KPIDefinitionId { get; private set; }
    public decimal Troskel { get; private set; }
    public string Mottagare { get; private set; } = "";
    public bool ArAktiv { get; private set; }

    private KPIAlert() { }

    public static KPIAlert Skapa(Guid kpiDefinitionId, decimal troskel, string mottagare, bool arAktiv = true)
    {
        return new KPIAlert
        {
            Id = Guid.NewGuid(),
            KPIDefinitionId = kpiDefinitionId,
            Troskel = troskel,
            Mottagare = mottagare,
            ArAktiv = arAktiv
        };
    }

    public void ToggleAktiv() { ArAktiv = !ArAktiv; }
}
