namespace RegionHR.Analytics.Domain;

/// <summary>
/// Planeringsscenario för workforce-planering.
/// Innehåller antaganden om headcount, attrition, löneutveckling etc.
/// </summary>
public class PlanningScenario
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public int BasÅr { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft/Active/Archived
    public string SkapadAv { get; private set; } = "";
    public DateTime SkapadVid { get; private set; }
    public List<ScenarioAssumption> Antaganden { get; private set; } = [];
    public List<ScenarioResult> Resultat { get; private set; } = [];

    private PlanningScenario() { }

    public static PlanningScenario Skapa(string namn, string beskrivning, int basÅr, string skapadAv)
    {
        return new PlanningScenario
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            BasÅr = basÅr,
            Status = "Draft",
            SkapadAv = skapadAv,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Aktivera()
    {
        if (Status == "Active")
            throw new InvalidOperationException("Scenariot är redan aktivt.");
        Status = "Active";
    }

    public void Arkivera()
    {
        if (Status == "Archived")
            throw new InvalidOperationException("Scenariot är redan arkiverat.");
        Status = "Archived";
    }
}
