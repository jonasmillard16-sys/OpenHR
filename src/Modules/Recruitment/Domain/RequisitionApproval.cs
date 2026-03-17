namespace RegionHR.Recruitment.Domain;

public enum RequisitionStatus { VantarGodkannande, Godkand, Nekad }

public class RequisitionApproval
{
    public Guid Id { get; private set; }
    public Guid VakansId { get; private set; }
    public Guid GodkannareId { get; private set; }
    public RequisitionStatus Status { get; private set; }
    public string? Kommentar { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? BeslutVid { get; private set; }

    private RequisitionApproval() { }

    public static RequisitionApproval Skapa(Guid vakansId, Guid godkannareId)
    {
        return new RequisitionApproval
        {
            Id = Guid.NewGuid(), VakansId = vakansId, GodkannareId = godkannareId,
            Status = RequisitionStatus.VantarGodkannande, SkapadVid = DateTime.UtcNow
        };
    }

    public void Godkann(string? kommentar = null) { Status = RequisitionStatus.Godkand; Kommentar = kommentar; BeslutVid = DateTime.UtcNow; }
    public void Neka(string kommentar) { Status = RequisitionStatus.Nekad; Kommentar = kommentar; BeslutVid = DateTime.UtcNow; }
}
