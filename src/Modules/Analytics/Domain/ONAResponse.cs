namespace RegionHR.Analytics.Domain;

/// <summary>
/// Svar på ONA-undersökning — en nominering/bedömning av en kollega.
/// </summary>
public sealed class ONAResponse
{
    public Guid Id { get; private set; }
    public Guid SurveyId { get; private set; }
    public Guid RespondentId { get; private set; }
    public Guid NomineradId { get; private set; }
    public int FrageIndex { get; private set; }
    public int Varde { get; private set; } // 1-5
    public DateTime SkapadVid { get; private set; }

    private ONAResponse() { } // EF Core

    public static ONAResponse Skapa(Guid surveyId, Guid respondentId, Guid nomineradId, int frageIndex, int varde)
    {
        if (surveyId == Guid.Empty) throw new ArgumentException("SurveyId krävs.", nameof(surveyId));
        if (respondentId == Guid.Empty) throw new ArgumentException("RespondentId krävs.", nameof(respondentId));
        if (nomineradId == Guid.Empty) throw new ArgumentException("NomineradId krävs.", nameof(nomineradId));
        if (varde < 1 || varde > 5) throw new ArgumentOutOfRangeException(nameof(varde), "Värde måste vara mellan 1 och 5.");

        return new ONAResponse
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            RespondentId = respondentId,
            NomineradId = nomineradId,
            FrageIndex = frageIndex,
            Varde = varde,
            SkapadVid = DateTime.UtcNow
        };
    }
}
