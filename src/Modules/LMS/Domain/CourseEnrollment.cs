namespace RegionHR.LMS.Domain;

public enum EnrollmentProgress { Anmalad, Paborjad, Genomford, Underkand, Avbruten }

public class CourseEnrollment
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid CourseId { get; private set; }
    public EnrollmentProgress Progress { get; private set; }
    public int? Resultat { get; private set; } // 0-100 score
    public bool Godkand { get; private set; }
    public DateTime AnmalanVid { get; private set; }
    public DateTime? PaborjadVid { get; private set; }
    public DateTime? GenomfordVid { get; private set; }
    public DateOnly? GiltigTill { get; private set; }

    private CourseEnrollment() { }

    public static CourseEnrollment Anmala(Guid anstallId, Guid courseId)
    {
        return new CourseEnrollment
        {
            Id = Guid.NewGuid(), AnstallId = anstallId, CourseId = courseId,
            Progress = EnrollmentProgress.Anmalad, AnmalanVid = DateTime.UtcNow
        };
    }

    public void Paborja() { Progress = EnrollmentProgress.Paborjad; PaborjadVid = DateTime.UtcNow; }

    public void Genomfor(int resultat, int? giltighetManader = null)
    {
        if (resultat < 0 || resultat > 100) throw new ArgumentOutOfRangeException(nameof(resultat));
        Progress = resultat >= 70 ? EnrollmentProgress.Genomford : EnrollmentProgress.Underkand;
        Resultat = resultat;
        Godkand = resultat >= 70;
        GenomfordVid = DateTime.UtcNow;
        if (giltighetManader.HasValue)
            GiltigTill = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(giltighetManader.Value));
    }

    public void Avbryt() { Progress = EnrollmentProgress.Avbruten; }
}
