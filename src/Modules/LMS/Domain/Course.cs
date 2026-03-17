namespace RegionHR.LMS.Domain;

public enum CourseFormat { Klassrum, Elearning, Blandat, Workshop }
public enum CourseStatus { Utkast, Publicerad, Arkiverad }

public class Course
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public CourseFormat Format { get; private set; }
    public CourseStatus Status { get; private set; }
    public int LangdMinuter { get; private set; }
    public bool ArObligatorisk { get; private set; }
    public string? Kategori { get; private set; }
    public int? GiltighetManader { get; private set; } // null = permanent
    public int MaxDeltagare { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private Course() { }

    public static Course Skapa(string namn, string beskrivning, CourseFormat format, int langdMinuter, bool obligatorisk, string? kategori = null, int? giltighetManader = null, int maxDeltagare = 0)
    {
        return new Course
        {
            Id = Guid.NewGuid(), Namn = namn, Beskrivning = beskrivning,
            Format = format, Status = CourseStatus.Utkast,
            LangdMinuter = langdMinuter, ArObligatorisk = obligatorisk,
            Kategori = kategori, GiltighetManader = giltighetManader,
            MaxDeltagare = maxDeltagare, SkapadVid = DateTime.UtcNow
        };
    }

    public void Publicera() { Status = CourseStatus.Publicerad; }
    public void Arkivera() { Status = CourseStatus.Arkiverad; }
}
