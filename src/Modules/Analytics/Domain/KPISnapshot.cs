namespace RegionHR.Analytics.Domain;

public class KPISnapshot
{
    public Guid Id { get; private set; }
    public Guid KPIDefinitionId { get; private set; }
    public string Period { get; private set; } = ""; // e.g. "2026-Q1"
    public decimal Varde { get; private set; }
    public decimal? JamforelseVarde { get; private set; }
    public string Trend { get; private set; } = ""; // Up/Down/Stable
    public DateTime BeraknadVid { get; private set; }

    private KPISnapshot() { }

    public static KPISnapshot Skapa(
        Guid kpiDefinitionId, string period,
        decimal varde, decimal? jamforelseVarde, string trend)
    {
        return new KPISnapshot
        {
            Id = Guid.NewGuid(),
            KPIDefinitionId = kpiDefinitionId,
            Period = period,
            Varde = varde,
            JamforelseVarde = jamforelseVarde,
            Trend = trend,
            BeraknadVid = DateTime.UtcNow
        };
    }
}
