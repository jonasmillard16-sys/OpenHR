namespace RegionHR.Competence.Domain;

public enum SkillCategory
{
    Klinisk,
    Teknisk,
    Ledarskap,
    Administration
}

/// <summary>
/// En normaliserad kompetens i organisationens skills-katalog.
/// Inte samma som en Certification — en skill är en förmåga/kunskap,
/// en certification är ett formellt dokument med utfärdare och giltighetsdatum.
/// </summary>
public class Skill
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public SkillCategory Kategori { get; private set; }
    public string? Beskrivning { get; private set; }

    private Skill() { }

    public static Skill Skapa(string namn, SkillCategory kategori, string? beskrivning = null)
    {
        return new Skill
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Kategori = kategori,
            Beskrivning = beskrivning
        };
    }
}
