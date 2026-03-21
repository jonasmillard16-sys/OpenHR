namespace RegionHR.Analytics.Domain;

public enum ONASurveyStatus { Draft, Open, Closed, Analyzed }

/// <summary>
/// ONA-undersökning (Organizational Network Analysis) — enkätbaserad.
/// </summary>
public sealed class ONASurvey
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string Period { get; private set; } = default!;
    public ONASurveyStatus Status { get; private set; }
    public string Fragor { get; private set; } = "[]"; // JSON array of questions
    public DateTime SkapadVid { get; private set; }

    private ONASurvey() { } // EF Core

    public static ONASurvey Skapa(string namn, string period, string fragor = "[]")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namn);
        ArgumentException.ThrowIfNullOrWhiteSpace(period);

        return new ONASurvey
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Period = period,
            Fragor = fragor,
            Status = ONASurveyStatus.Draft,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Öppnar undersökningen för svar.
    /// </summary>
    public void Oppna()
    {
        if (Status != ONASurveyStatus.Draft)
            throw new InvalidOperationException($"Kan bara öppna utkast. Nuvarande status: {Status}.");

        Status = ONASurveyStatus.Open;
    }

    /// <summary>
    /// Stänger undersökningen.
    /// </summary>
    public void Stang()
    {
        if (Status != ONASurveyStatus.Open)
            throw new InvalidOperationException($"Kan bara stänga öppna undersökningar. Nuvarande status: {Status}.");

        Status = ONASurveyStatus.Closed;
    }

    /// <summary>
    /// Markerar undersökningen som analyserad.
    /// </summary>
    public void Analysera()
    {
        if (Status != ONASurveyStatus.Closed)
            throw new InvalidOperationException($"Kan bara analysera stängda undersökningar. Nuvarande status: {Status}.");

        Status = ONASurveyStatus.Analyzed;
    }
}
