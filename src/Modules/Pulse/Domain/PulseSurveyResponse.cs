namespace RegionHR.Pulse.Domain;

/// <summary>
/// Ett anonymt svar på en pulsundersökning.
/// Innehåller INGEN koppling till anställd/användare — svaren är helt anonyma.
/// Systemet kan inte verifiera vem som svarat och samma person kan svara flera gånger i v1.
/// </summary>
public sealed class PulseSurveyResponse
{
    public Guid Id { get; private set; }
    public Guid SurveyId { get; private set; }
    public DateTime SvaradVid { get; private set; }

    private readonly List<PulseSurveyAnswer> _svar = [];
    public IReadOnlyList<PulseSurveyAnswer> Svar => _svar.AsReadOnly();

    private PulseSurveyResponse() { }

    public static PulseSurveyResponse Skapa(Guid surveyId)
    {
        return new PulseSurveyResponse
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            SvaradVid = DateTime.UtcNow
        };
    }

    public void LaggTillSvar(Guid fragaId, int varde, string? kommentar = null)
    {
        if (varde < 1 || varde > 5)
            throw new ArgumentOutOfRangeException(nameof(varde), "Värde måste vara 1–5.");

        _svar.Add(new PulseSurveyAnswer
        {
            Id = Guid.NewGuid(),
            FragaId = fragaId,
            Varde = varde,
            Kommentar = kommentar
        });
    }
}

public sealed class PulseSurveyAnswer
{
    public Guid Id { get; set; }
    public Guid FragaId { get; set; }
    public int Varde { get; set; }
    public string? Kommentar { get; set; }
}
