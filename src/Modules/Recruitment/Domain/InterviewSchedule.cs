namespace RegionHR.Recruitment.Domain;

public class InterviewSchedule
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public DateTime Tidpunkt { get; private set; }
    public int LangdMinuter { get; private set; }
    public string Plats { get; private set; } = "";
    public List<Guid> InterviewerIds { get; private set; } = new();
    public string? Anteckningar { get; private set; }
    public bool Genomford { get; private set; }

    private InterviewSchedule() { }

    public static InterviewSchedule Skapa(Guid applicationId, DateTime tidpunkt, int langdMinuter, string plats, List<Guid>? interviewerIds = null)
    {
        return new InterviewSchedule
        {
            Id = Guid.NewGuid(), ApplicationId = applicationId, Tidpunkt = tidpunkt,
            LangdMinuter = langdMinuter, Plats = plats,
            InterviewerIds = interviewerIds ?? new List<Guid>()
        };
    }

    public void MarkeraSomGenomford(string? anteckningar = null) { Genomford = true; Anteckningar = anteckningar; }
}
