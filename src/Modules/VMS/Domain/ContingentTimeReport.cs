namespace RegionHR.VMS.Domain;

/// <summary>
/// Tidrapport för inhyrd personal — attesteras av beställare.
/// </summary>
public sealed class ContingentTimeReport
{
    public Guid Id { get; private set; }
    public Guid ContingentWorkerId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public decimal Timmar { get; private set; }
    public decimal OBTimmar { get; private set; }
    public decimal Overtid { get; private set; }
    public Guid? AtesteradAv { get; private set; }
    public TimeReportStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ContingentTimeReport() { } // EF Core

    public static ContingentTimeReport Skapa(
        Guid contingentWorkerId,
        string period,
        decimal timmar,
        decimal obTimmar,
        decimal overtid)
    {
        return new ContingentTimeReport
        {
            Id = Guid.NewGuid(),
            ContingentWorkerId = contingentWorkerId,
            Period = period,
            Timmar = timmar,
            OBTimmar = obTimmar,
            Overtid = overtid,
            Status = TimeReportStatus.Draft
        };
    }

    public void SkickaIn()
    {
        if (Status != TimeReportStatus.Draft)
            throw new InvalidOperationException("Kan bara skicka in utkast.");
        Status = TimeReportStatus.Submitted;
    }

    public void Attestera(Guid attestantId)
    {
        if (Status != TimeReportStatus.Submitted)
            throw new InvalidOperationException("Kan bara attestera inskickade tidrapporter.");
        Status = TimeReportStatus.Attested;
        AtesteradAv = attestantId;
    }
}
