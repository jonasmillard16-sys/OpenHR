namespace RegionHR.Pulse.Domain;

public enum PulseSurveyStatus
{
    Utkast,
    Oppnad,
    Stangd
}

public sealed class PulseSurvey
{
    public Guid Id { get; private set; }
    public string Titel { get; private set; } = default!;
    public string? Beskrivning { get; private set; }
    public PulseSurveyStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public string SkapadAv { get; private set; } = default!;
    public DateTime? OppnadVid { get; private set; }
    public DateTime? StangdVid { get; private set; }

    private readonly List<PulseSurveyQuestion> _fragor = [];
    public IReadOnlyList<PulseSurveyQuestion> Fragor => _fragor.AsReadOnly();

    private PulseSurvey() { }

    public static PulseSurvey Skapa(string titel, string? beskrivning, string skapadAv)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titel);
        ArgumentException.ThrowIfNullOrWhiteSpace(skapadAv);

        return new PulseSurvey
        {
            Id = Guid.NewGuid(),
            Titel = titel,
            Beskrivning = beskrivning,
            Status = PulseSurveyStatus.Utkast,
            SkapadVid = DateTime.UtcNow,
            SkapadAv = skapadAv
        };
    }

    public void LaggTillFraga(string text, int ordning)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (Status != PulseSurveyStatus.Utkast)
            throw new InvalidOperationException("Kan bara lägga till frågor i utkast.");

        _fragor.Add(new PulseSurveyQuestion
        {
            Id = Guid.NewGuid(),
            Text = text,
            Ordning = ordning
        });
    }

    public void Oppna()
    {
        if (Status != PulseSurveyStatus.Utkast)
            throw new InvalidOperationException($"Kan bara öppna från Utkast. Nuvarande: {Status}");
        if (_fragor.Count == 0)
            throw new InvalidOperationException("Kan inte öppna enkät utan frågor.");

        Status = PulseSurveyStatus.Oppnad;
        OppnadVid = DateTime.UtcNow;
    }

    public void Stang()
    {
        if (Status != PulseSurveyStatus.Oppnad)
            throw new InvalidOperationException($"Kan bara stänga från Öppnad. Nuvarande: {Status}");

        Status = PulseSurveyStatus.Stangd;
        StangdVid = DateTime.UtcNow;
    }
}

public sealed class PulseSurveyQuestion
{
    public Guid Id { get; set; }
    public string Text { get; set; } = default!;
    public int Ordning { get; set; }
}
