namespace RegionHR.Analytics.Domain;

/// <summary>
/// Nätverkskant i ONA — en koppling mellan två anställda.
/// </summary>
public sealed class NetworkEdge
{
    public Guid Id { get; private set; }
    public Guid SurveyId { get; private set; }
    public Guid FranAnstallId { get; private set; }
    public Guid TillAnstallId { get; private set; }
    public int FrageIndex { get; private set; }
    public decimal Styrka { get; private set; }

    private NetworkEdge() { } // EF Core

    public static NetworkEdge Skapa(Guid surveyId, Guid franAnstallId, Guid tillAnstallId, int frageIndex, decimal styrka)
    {
        if (surveyId == Guid.Empty) throw new ArgumentException("SurveyId krävs.", nameof(surveyId));
        if (franAnstallId == Guid.Empty) throw new ArgumentException("FranAnstallId krävs.", nameof(franAnstallId));
        if (tillAnstallId == Guid.Empty) throw new ArgumentException("TillAnstallId krävs.", nameof(tillAnstallId));

        return new NetworkEdge
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            FranAnstallId = franAnstallId,
            TillAnstallId = tillAnstallId,
            FrageIndex = frageIndex,
            Styrka = styrka
        };
    }
}
