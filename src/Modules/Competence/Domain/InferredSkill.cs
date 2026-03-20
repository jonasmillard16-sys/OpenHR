namespace RegionHR.Competence.Domain;

/// <summary>
/// AI-härledd skill från befattning, kurs, certifiering eller erfarenhet.
/// Kan bekräftas manuellt av den anställda.
/// </summary>
public class InferredSkill
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid SkillId { get; private set; }

    /// <summary>Källa: Befattning, Kurs, Certifiering, Erfarenhet</summary>
    public string Kalla { get; private set; } = default!;

    /// <summary>Konfidens 0-100</summary>
    public int Konfidens { get; private set; }

    public bool ArBekraftad { get; private set; }

    private InferredSkill() { }

    public static InferredSkill Skapa(Guid anstallId, Guid skillId, string kalla, int konfidens)
    {
        if (konfidens < 0 || konfidens > 100)
            throw new ArgumentException("Konfidens måste vara 0-100", nameof(konfidens));

        return new InferredSkill
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            SkillId = skillId,
            Kalla = kalla,
            Konfidens = konfidens,
            ArBekraftad = false
        };
    }

    public void Bekrafta()
    {
        ArBekraftad = true;
        Konfidens = 100;
    }
}
