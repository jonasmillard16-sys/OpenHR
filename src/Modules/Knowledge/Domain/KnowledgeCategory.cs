namespace RegionHR.Knowledge.Domain;

public class KnowledgeCategory
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public int Ordning { get; private set; }
    public string Ikon { get; private set; } = "";

    private KnowledgeCategory() { }

    public static KnowledgeCategory Skapa(string namn, string beskrivning, int ordning, string ikon)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn krävs", nameof(namn));

        return new KnowledgeCategory
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            Ordning = ordning,
            Ikon = ikon
        };
    }

    public void Uppdatera(string namn, string beskrivning, int ordning, string ikon)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn krävs", nameof(namn));

        Namn = namn;
        Beskrivning = beskrivning;
        Ordning = ordning;
        Ikon = ikon;
    }
}
