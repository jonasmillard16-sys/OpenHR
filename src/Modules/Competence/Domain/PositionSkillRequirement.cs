namespace RegionHR.Competence.Domain;

/// <summary>
/// Kopplar en Position till en krävd skill med miniminivå.
/// Ersätter Position.KravdaKompetenser (List&lt;string&gt;) som källa
/// för kravprofiler i gap-analysen.
/// </summary>
public class PositionSkillRequirement
{
    public Guid Id { get; private set; }
    public Guid PositionId { get; private set; }
    public Guid SkillId { get; private set; }

    /// <summary>Miniminivå 1-5 som krävs för positionen</summary>
    public int MinNiva { get; private set; }

    private PositionSkillRequirement() { }

    public static PositionSkillRequirement Skapa(Guid positionId, Guid skillId, int minNiva)
    {
        if (minNiva < 1 || minNiva > 5)
            throw new ArgumentException("MinNivå måste vara 1-5", nameof(minNiva));

        return new PositionSkillRequirement
        {
            Id = Guid.NewGuid(),
            PositionId = positionId,
            SkillId = skillId,
            MinNiva = minNiva
        };
    }
}
