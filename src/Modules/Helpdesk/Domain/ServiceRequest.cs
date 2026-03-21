using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// Serviceärende i HR helpdesk. Aggregate root för helpdesk-modulen.
/// Hanterar SLA-spårning, köer, agenthantering och nöjdhetspoäng.
/// </summary>
public sealed class ServiceRequest : AggregateRoot<Guid>
{
    public string Titel { get; private set; } = string.Empty;
    public string Beskrivning { get; private set; } = string.Empty;
    public Guid KategoriId { get; private set; }
    public ServiceRequestPriority Prioritet { get; private set; }
    public ServiceRequestStatus Status { get; private set; }
    public string KallKanal { get; private set; } = string.Empty; // Portal/Email/Chat/Assistant
    public EmployeeId InrapportadAv { get; private set; }
    public Guid? TilldeladAgent { get; private set; }
    public Guid? TilldeladKo { get; private set; }
    public Guid? SLADefinitionId { get; private set; }
    public DateTime? SLADeadline { get; private set; }
    public DateTime? LostVid { get; private set; }
    public DateTime? StangdVid { get; private set; }
    public int? NojdhetsPoang { get; private set; }

    private readonly List<ServiceRequestComment> _kommentarer = [];
    public IReadOnlyList<ServiceRequestComment> Kommentarer => _kommentarer.AsReadOnly();

    private readonly List<SLAMilestone> _slaMilestones = [];
    public IReadOnlyList<SLAMilestone> SLAMilestones => _slaMilestones.AsReadOnly();

    private ServiceRequest() { }

    public static ServiceRequest Skapa(
        string titel,
        string beskrivning,
        Guid kategoriId,
        ServiceRequestPriority prioritet,
        string kallKanal,
        EmployeeId inrapportadAv)
    {
        return new ServiceRequest
        {
            Id = Guid.NewGuid(),
            Titel = titel,
            Beskrivning = beskrivning,
            KategoriId = kategoriId,
            Prioritet = prioritet,
            Status = ServiceRequestStatus.New,
            KallKanal = kallKanal,
            InrapportadAv = inrapportadAv
        };
    }

    public void Tilldela(Guid agentId)
    {
        TilldeladAgent = agentId;
        if (Status == ServiceRequestStatus.New)
            Status = ServiceRequestStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TilldelaKo(Guid koId)
    {
        TilldeladKo = koId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PaborjaArbete()
    {
        if (Status != ServiceRequestStatus.Assigned && Status != ServiceRequestStatus.New)
            throw new InvalidOperationException("Kan bara påbörja arbete från status Ny eller Tilldelad");

        Status = ServiceRequestStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VantaPaAntalld()
    {
        Status = ServiceRequestStatus.WaitingOnEmployee;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Los(string losning)
    {
        Status = ServiceRequestStatus.Resolved;
        LostVid = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _kommentarer.Add(new ServiceRequestComment
        {
            Id = Guid.NewGuid(),
            ServiceRequestId = Id,
            Innehall = losning,
            ArIntern = false,
            SkapadVid = DateTime.UtcNow
        });

        RaiseDomainEvent(new ServiceRequestResolvedEvent(Id, InrapportadAv));
    }

    public void Stang()
    {
        Status = ServiceRequestStatus.Closed;
        StangdVid = DateTime.UtcNow;
        if (LostVid is null) LostVid = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SattNojdhet(int poang)
    {
        if (poang < 1 || poang > 5)
            throw new ArgumentOutOfRangeException(nameof(poang), "Nöjdhetspoäng måste vara mellan 1 och 5");
        NojdhetsPoang = poang;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LaggTillKommentar(EmployeeId? forfattareId, string innehall, bool arIntern)
    {
        _kommentarer.Add(new ServiceRequestComment
        {
            Id = Guid.NewGuid(),
            ServiceRequestId = Id,
            ForfattareId = forfattareId,
            Innehall = innehall,
            ArIntern = arIntern,
            SkapadVid = DateTime.UtcNow
        });
        UpdatedAt = DateTime.UtcNow;
    }

    public void StallInSLA(Guid slaDefinitionId, DateTime deadline)
    {
        SLADefinitionId = slaDefinitionId;
        SLADeadline = deadline;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LaggTillSLAMilestone(SLAMilestone milestone)
    {
        _slaMilestones.Add(milestone);
    }
}

public enum ServiceRequestPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ServiceRequestStatus
{
    New,
    Assigned,
    InProgress,
    WaitingOnEmployee,
    Resolved,
    Closed
}

public sealed record ServiceRequestResolvedEvent(Guid ServiceRequestId, EmployeeId InrapportadAv) : DomainEvent;
