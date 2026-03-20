using RegionHR.SharedKernel.Domain;

namespace RegionHR.Competence.Domain;

public enum OpportunityStatus
{
    Draft,
    Published,
    Closed,
    Filled
}

public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    Accepted,
    Rejected
}

/// <summary>
/// Intern möjlighet: roll, projekt, gig, mentorskap eller rotation.
/// </summary>
public class InternalOpportunity
{
    public InternalOpportunityId Id { get; private set; }

    /// <summary>Roll, Projekt, Gig, Mentorskap, Rotation</summary>
    public string Typ { get; private set; } = default!;

    public string Titel { get; private set; } = default!;
    public Guid EnhetId { get; private set; }
    public DateOnly? PeriodFran { get; private set; }
    public DateOnly? PeriodTill { get; private set; }

    /// <summary>JSON med kravprofil</summary>
    public string? Kravprofil { get; private set; }

    public OpportunityStatus Status { get; private set; }

    private readonly List<OpportunityApplication> _ansokningar = [];
    public IReadOnlyList<OpportunityApplication> Ansokningar => _ansokningar.AsReadOnly();

    private InternalOpportunity() { }

    public static InternalOpportunity Skapa(string typ, string titel, Guid enhetId,
        DateOnly? periodFran = null, DateOnly? periodTill = null, string? kravprofil = null)
    {
        return new InternalOpportunity
        {
            Id = InternalOpportunityId.New(),
            Typ = typ,
            Titel = titel,
            EnhetId = enhetId,
            PeriodFran = periodFran,
            PeriodTill = periodTill,
            Kravprofil = kravprofil,
            Status = OpportunityStatus.Draft
        };
    }

    public void Publicera()
    {
        if (Status != OpportunityStatus.Draft)
            throw new InvalidOperationException("Kan bara publicera utkast");
        Status = OpportunityStatus.Published;
    }

    public void Stang()
    {
        if (Status != OpportunityStatus.Published)
            throw new InvalidOperationException("Kan bara stänga publicerade möjligheter");
        Status = OpportunityStatus.Closed;
    }

    public void Tillsatt()
    {
        if (Status != OpportunityStatus.Published && Status != OpportunityStatus.Closed)
            throw new InvalidOperationException("Kan bara tillsätta publicerade eller stängda möjligheter");
        Status = OpportunityStatus.Filled;
    }
}

/// <summary>
/// Ansökan till en intern möjlighet.
/// </summary>
public class OpportunityApplication
{
    public Guid Id { get; private set; }
    public InternalOpportunityId InternalOpportunityId { get; private set; }
    public Guid AnstallId { get; private set; }
    public string? Motivering { get; private set; }
    public int MatchScore { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private OpportunityApplication() { }

    public static OpportunityApplication Skapa(InternalOpportunityId opportunityId, Guid anstallId,
        string? motivering, int matchScore = 0)
    {
        if (matchScore < 0 || matchScore > 100)
            throw new ArgumentException("MatchScore måste vara 0-100", nameof(matchScore));

        return new OpportunityApplication
        {
            Id = Guid.NewGuid(),
            InternalOpportunityId = opportunityId,
            AnstallId = anstallId,
            Motivering = motivering,
            MatchScore = matchScore,
            Status = ApplicationStatus.Submitted,
            SkapadVid = DateTime.UtcNow
        };
    }
}
