using RegionHR.SharedKernel.Domain;

namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// Kommentar på ett serviceärende. Kan vara intern (dold för anställd)
/// eller publik (synlig i tidslinje).
/// </summary>
public sealed class ServiceRequestComment
{
    public Guid Id { get; set; }
    public Guid ServiceRequestId { get; set; }
    public EmployeeId? ForfattareId { get; set; }
    public string Innehall { get; set; } = string.Empty;
    public bool ArIntern { get; set; }
    public DateTime SkapadVid { get; set; }
}
