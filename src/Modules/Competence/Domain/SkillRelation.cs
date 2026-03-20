namespace RegionHR.Competence.Domain;

/// <summary>
/// Relation mellan två skills (prerequisite, related, supersedes).
/// </summary>
public class SkillRelation
{
    public Guid Id { get; private set; }
    public Guid FranSkillId { get; private set; }
    public Guid TillSkillId { get; private set; }

    /// <summary>Prerequisite, Related, eller Supersedes</summary>
    public string Typ { get; private set; } = default!;

    private SkillRelation() { }

    public static SkillRelation Skapa(Guid franSkillId, Guid tillSkillId, string typ)
    {
        if (string.IsNullOrWhiteSpace(typ))
            throw new ArgumentException("Typ krävs", nameof(typ));

        return new SkillRelation
        {
            Id = Guid.NewGuid(),
            FranSkillId = franSkillId,
            TillSkillId = tillSkillId,
            Typ = typ
        };
    }
}
