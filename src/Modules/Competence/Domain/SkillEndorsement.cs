namespace RegionHR.Competence.Domain;

/// <summary>
/// Endorsement av en kollegas skill.
/// </summary>
public class SkillEndorsement
{
    public Guid Id { get; private set; }
    public Guid SkillId { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid BekraftadAv { get; private set; }
    public DateTime Datum { get; private set; }

    private SkillEndorsement() { }

    public static SkillEndorsement Skapa(Guid skillId, Guid anstallId, Guid bekraftadAv)
    {
        return new SkillEndorsement
        {
            Id = Guid.NewGuid(),
            SkillId = skillId,
            AnstallId = anstallId,
            BekraftadAv = bekraftadAv,
            Datum = DateTime.UtcNow
        };
    }
}
