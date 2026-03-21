namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// SLA-milstolpe (svar/lösning) med mål- och faktisk tid.
/// Spårar om milstolpen uppfylldes inom SLA.
/// </summary>
public sealed class SLAMilestone
{
    public Guid Id { get; set; }
    public Guid ServiceRequestId { get; set; }
    public string Typ { get; set; } = string.Empty; // "Response" eller "Resolution"
    public DateTime MalTid { get; set; }
    public DateTime? FaktiskTid { get; set; }
    public bool? ArUppfylld { get; set; }

    public static SLAMilestone Skapa(Guid serviceRequestId, string typ, DateTime malTid)
    {
        return new SLAMilestone
        {
            Id = Guid.NewGuid(),
            ServiceRequestId = serviceRequestId,
            Typ = typ,
            MalTid = malTid
        };
    }

    public void Uppfyll(DateTime faktiskTid)
    {
        FaktiskTid = faktiskTid;
        ArUppfylld = faktiskTid <= MalTid;
    }
}
