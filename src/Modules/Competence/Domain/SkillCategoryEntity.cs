namespace RegionHR.Competence.Domain;

/// <summary>
/// Normaliserad skill-kategori som entitet (ersätter enum SkillCategory).
/// Enum-propertyn behålls på Skill för bakåtkompatibilitet.
/// </summary>
public class SkillCategoryEntity
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string? Beskrivning { get; private set; }

    private SkillCategoryEntity() { }

    public static SkillCategoryEntity Skapa(string namn, string? beskrivning = null)
    {
        return new SkillCategoryEntity
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning
        };
    }
}
