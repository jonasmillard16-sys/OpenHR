namespace RegionHR.LMS.Domain;

public class LearningPath
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public string? RollNamn { get; private set; }
    public List<LearningPathStep> Steg { get; private set; } = new();
    public DateTime SkapadVid { get; private set; }

    private LearningPath() { }

    public static LearningPath Skapa(string namn, string beskrivning, string? rollNamn = null)
    {
        return new LearningPath { Id = Guid.NewGuid(), Namn = namn, Beskrivning = beskrivning, RollNamn = rollNamn, SkapadVid = DateTime.UtcNow };
    }

    public void LaggTillSteg(Guid courseId, int ordning, bool obligatorisk = true)
    {
        Steg.Add(new LearningPathStep(courseId, ordning, obligatorisk));
    }
}

public class LearningPathStep
{
    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }
    public int Ordning { get; private set; }
    public bool Obligatorisk { get; private set; }

    private LearningPathStep() { }
    public LearningPathStep(Guid courseId, int ordning, bool obligatorisk)
    {
        Id = Guid.NewGuid(); CourseId = courseId; Ordning = ordning; Obligatorisk = obligatorisk;
    }
}
