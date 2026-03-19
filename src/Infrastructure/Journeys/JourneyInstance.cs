namespace RegionHR.Infrastructure.Journeys;

public enum JourneyInstanceStatus { Startad, Pagaende, Slutford }

/// <summary>
/// En specifik journey startad för en specifik anställd.
/// Bär sin egen snapshot av stegen — oberoende av mallen efter skapande.
/// </summary>
public class JourneyInstance
{
    public Guid Id { get; private set; }
    public Guid TemplateId { get; private set; }
    public string MallNamn { get; private set; } = default!;
    public Guid AnstallId { get; private set; }
    public string AnstallNamn { get; private set; } = default!;
    public DateTime Startdatum { get; private set; }
    public JourneyInstanceStatus Status { get; private set; }

    private readonly List<JourneyStepInstance> _steg = [];
    public IReadOnlyList<JourneyStepInstance> Steg => _steg.AsReadOnly();

    /// <summary>Beräknad progress 0-100. Lagras inte.</summary>
    public int Progress => _steg.Count > 0
        ? (int)Math.Round(_steg.Count(s => s.Klar) / (double)_steg.Count * 100)
        : 0;

    private JourneyInstance() { }

    /// <summary>
    /// Skapar en ny journey-instans genom att kopiera steg från mallen.
    /// Stegen lever sedan oberoende av mallen — malländringar påverkar inte instansen.
    /// </summary>
    public static JourneyInstance SkapaFranMall(
        JourneyTemplate mall, Guid anstallId, string anstallNamn, DateTime startdatum)
    {
        var instance = new JourneyInstance
        {
            Id = Guid.NewGuid(),
            TemplateId = mall.Id,
            MallNamn = mall.Namn,
            AnstallId = anstallId,
            AnstallNamn = anstallNamn,
            Startdatum = startdatum,
            Status = JourneyInstanceStatus.Startad
        };

        foreach (var stegMall in mall.Steg.OrderBy(s => s.Ordning))
        {
            instance._steg.Add(JourneyStepInstance.SkapaFranMall(stegMall));
        }

        return instance;
    }

    public void MarkeraStegKlart(Guid stegId, string klarAv)
    {
        var steg = _steg.FirstOrDefault(s => s.Id == stegId)
            ?? throw new ArgumentException($"Steg {stegId} finns inte i denna journey");
        steg.MarkeraKlar(klarAv);

        Status = _steg.All(s => s.Klar)
            ? JourneyInstanceStatus.Slutford
            : JourneyInstanceStatus.Pagaende;
    }

    /// <summary>
    /// Seed/test: skapa med givet startdatum utan mall-referens.
    /// </summary>
    internal static JourneyInstance SkapaForSeed(
        Guid templateId, string mallNamn, Guid anstallId, string anstallNamn, DateTime startdatum)
    {
        return new JourneyInstance
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            MallNamn = mallNamn,
            AnstallId = anstallId,
            AnstallNamn = anstallNamn,
            Startdatum = startdatum,
            Status = JourneyInstanceStatus.Startad
        };
    }

    internal void LaggTillStegForSeed(JourneyStepInstance steg) => _steg.Add(steg);
}

/// <summary>
/// Status för ett specifikt steg i en journey-instans.
/// Owned entity — ingen egen DbSet.
/// Snapshot kopierad från JourneyStepTemplate vid instansiering.
/// </summary>
public class JourneyStepInstance
{
    public Guid Id { get; private set; }
    public int Ordning { get; private set; }
    public string Titel { get; private set; } = default!;
    public string Beskrivning { get; private set; } = default!;
    public string AnsvarigRoll { get; private set; } = default!;
    public int DagOffset { get; private set; }
    public bool Klar { get; private set; }
    public DateTime? KlarVid { get; private set; }
    public string? KlarAv { get; private set; }

    private JourneyStepInstance() { }

    internal static JourneyStepInstance SkapaFranMall(JourneyStepTemplate mall)
    {
        return new JourneyStepInstance
        {
            Id = Guid.NewGuid(),
            Ordning = mall.Ordning,
            Titel = mall.Titel,
            Beskrivning = mall.Beskrivning,
            AnsvarigRoll = mall.AnsvarigRoll,
            DagOffset = mall.DagOffset,
            Klar = false
        };
    }

    internal static JourneyStepInstance SkapaForSeed(
        int ordning, string titel, string beskrivning, string ansvarigRoll, int dagOffset, bool klar = false)
    {
        return new JourneyStepInstance
        {
            Id = Guid.NewGuid(),
            Ordning = ordning,
            Titel = titel,
            Beskrivning = beskrivning,
            AnsvarigRoll = ansvarigRoll,
            DagOffset = dagOffset,
            Klar = klar,
            KlarVid = klar ? DateTime.UtcNow : null,
            KlarAv = klar ? "Seed" : null
        };
    }

    internal void MarkeraKlar(string klarAv)
    {
        Klar = true;
        KlarVid = DateTime.UtcNow;
        KlarAv = klarAv;
    }
}
