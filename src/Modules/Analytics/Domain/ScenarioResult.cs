namespace RegionHR.Analytics.Domain;

/// <summary>
/// Beräknat resultat för en period i ett planeringsscenario.
/// Visar headcount-prognos, FTE, lönekostnad, AG-avgifter och budgetavvikelse.
/// </summary>
public class ScenarioResult
{
    public Guid Id { get; private set; }
    public Guid ScenarioId { get; private set; }
    public string Period { get; private set; } = ""; // t.ex. "2026-04", "2026-Q2"
    public int HeadcountPrognos { get; private set; }
    public decimal FTEPrognos { get; private set; }
    public decimal TotalLönekostnad { get; private set; }
    public decimal AGAvgifter { get; private set; }
    public decimal TotalKostnad { get; private set; }
    public decimal DeltaMotBudget { get; private set; }
    public DateTime BeräknadVid { get; private set; }

    private ScenarioResult() { }

    public static ScenarioResult Skapa(
        Guid scenarioId,
        string period,
        int headcountPrognos,
        decimal ftePrognos,
        decimal totalLönekostnad,
        decimal agAvgifter,
        decimal totalKostnad,
        decimal deltaMotBudget)
    {
        return new ScenarioResult
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenarioId,
            Period = period,
            HeadcountPrognos = headcountPrognos,
            FTEPrognos = ftePrognos,
            TotalLönekostnad = totalLönekostnad,
            AGAvgifter = agAvgifter,
            TotalKostnad = totalKostnad,
            DeltaMotBudget = deltaMotBudget,
            BeräknadVid = DateTime.UtcNow
        };
    }
}
