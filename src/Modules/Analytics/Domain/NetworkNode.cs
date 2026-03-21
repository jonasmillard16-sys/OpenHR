namespace RegionHR.Analytics.Domain;

/// <summary>
/// Nätverksnod i ONA — en anställd med beräknade centralitetsmått.
/// </summary>
public sealed class NetworkNode
{
    public Guid Id { get; private set; }
    public Guid SurveyId { get; private set; }
    public Guid AnstallId { get; private set; }
    public int InDegree { get; private set; }
    public int OutDegree { get; private set; }
    public decimal BetweennessCentrality { get; private set; }
    public string? Kluster { get; private set; }
    public string Roll { get; private set; } = default!; // ValueCreator, Influencer, Bottleneck, BoundarySpanner, Isolated

    private NetworkNode() { } // EF Core

    public static NetworkNode Skapa(
        Guid surveyId,
        Guid anstallId,
        int inDegree,
        int outDegree,
        decimal betweennessCentrality,
        string? kluster,
        string roll)
    {
        if (surveyId == Guid.Empty) throw new ArgumentException("SurveyId krävs.", nameof(surveyId));
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));
        ArgumentException.ThrowIfNullOrWhiteSpace(roll);

        return new NetworkNode
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            AnstallId = anstallId,
            InDegree = inDegree,
            OutDegree = outDegree,
            BetweennessCentrality = betweennessCentrality,
            Kluster = kluster,
            Roll = roll
        };
    }
}
