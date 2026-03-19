namespace RegionHR.Infrastructure.Journeys;

public enum JourneyKategori
{
    Onboarding,
    NyChef,
    Foraldraledighet,
    Sjukfranvaro,
    Avslut
}

/// <summary>
/// Mall för en medarbetarresa. Definierar steg som kopieras till
/// JourneyInstance vid instansiering. Ändringar i mallen påverkar
/// inte redan skapade instanser.
/// </summary>
public class JourneyTemplate
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string? Beskrivning { get; private set; }
    public JourneyKategori Kategori { get; private set; }

    private readonly List<JourneyStepTemplate> _steg = [];
    public IReadOnlyList<JourneyStepTemplate> Steg => _steg.AsReadOnly();

    private JourneyTemplate() { }

    public static JourneyTemplate Skapa(string namn, JourneyKategori kategori, string? beskrivning = null)
    {
        return new JourneyTemplate
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Kategori = kategori,
            Beskrivning = beskrivning
        };
    }

    public void LaggTillSteg(string titel, string beskrivning, string ansvarigRoll, int dagOffset)
    {
        var ordning = _steg.Count + 1;
        _steg.Add(JourneyStepTemplate.Skapa(ordning, titel, beskrivning, ansvarigRoll, dagOffset));
    }
}

/// <summary>
/// Stegdefinition i en mall. Owned entity — ingen egen DbSet.
/// </summary>
public class JourneyStepTemplate
{
    public Guid Id { get; private set; }
    public int Ordning { get; private set; }
    public string Titel { get; private set; } = default!;
    public string Beskrivning { get; private set; } = default!;
    public string AnsvarigRoll { get; private set; } = default!;
    public int DagOffset { get; private set; }

    private JourneyStepTemplate() { }

    internal static JourneyStepTemplate Skapa(int ordning, string titel, string beskrivning, string ansvarigRoll, int dagOffset)
    {
        return new JourneyStepTemplate
        {
            Id = Guid.NewGuid(),
            Ordning = ordning,
            Titel = titel,
            Beskrivning = beskrivning,
            AnsvarigRoll = ansvarigRoll,
            DagOffset = dagOffset
        };
    }
}
