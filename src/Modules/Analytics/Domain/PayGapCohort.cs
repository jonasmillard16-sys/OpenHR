namespace RegionHR.Analytics.Domain;

public class PayGapCohort
{
    public Guid Id { get; private set; }
    public Guid PayGapAnalysisId { get; private set; }
    public string KohortNamn { get; private set; } = "";
    public int AntalAnstallda { get; private set; }
    public decimal GapProcent { get; private set; }
    public decimal? TrendFranForraAret { get; private set; }

    private PayGapCohort() { }

    public static PayGapCohort Skapa(
        Guid payGapAnalysisId,
        string kohortNamn,
        int antalAnstallda,
        decimal gapProcent,
        decimal? trendFranForraAret = null)
    {
        return new PayGapCohort
        {
            Id = Guid.NewGuid(),
            PayGapAnalysisId = payGapAnalysisId,
            KohortNamn = kohortNamn,
            AntalAnstallda = antalAnstallda,
            GapProcent = gapProcent,
            TrendFranForraAret = trendFranForraAret
        };
    }
}
