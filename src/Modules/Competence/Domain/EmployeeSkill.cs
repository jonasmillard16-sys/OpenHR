namespace RegionHR.Competence.Domain;

/// <summary>
/// Kopplar en anställd till en skill med proficiency-nivå.
/// Kopplas mot Employee (inte Employment) — skills tillhör personen,
/// inte anställningsposten, och följer med vid enhetsbyte.
/// </summary>
public class EmployeeSkill
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid SkillId { get; private set; }

    /// <summary>Proficiency 1-5 (1=grundläggande, 5=expert)</summary>
    public int Niva { get; private set; }

    private EmployeeSkill() { }

    public static EmployeeSkill Skapa(Guid anstallId, Guid skillId, int niva)
    {
        if (niva < 1 || niva > 5)
            throw new ArgumentException("Nivå måste vara 1-5", nameof(niva));

        return new EmployeeSkill
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            SkillId = skillId,
            Niva = niva
        };
    }
}
