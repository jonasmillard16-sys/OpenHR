using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Begäran om täckning av ett pass som inte kan bemannas som planerat.
/// Flöde: Open -> Offered -> Covered (eller Uncovered om ingen tar det).
/// </summary>
public sealed class ShiftCoverageRequest : Entity<Guid>
{
    public Guid ScheduledShiftId { get; private set; }
    public string Anledning { get; private set; } = string.Empty;
    public CoverageStatus Status { get; private set; }
    public EmployeeId? TilldeladAnstallId { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private ShiftCoverageRequest() { }

    /// <summary>
    /// Skapa en ny täckningsförfrågan.
    /// </summary>
    public static ShiftCoverageRequest Skapa(Guid scheduledShiftId, string anledning)
    {
        if (string.IsNullOrWhiteSpace(anledning))
            throw new ArgumentException("Anledning krävs.", nameof(anledning));

        return new ShiftCoverageRequest
        {
            Id = Guid.NewGuid(),
            ScheduledShiftId = scheduledShiftId,
            Anledning = anledning,
            Status = CoverageStatus.Open,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tilldela en anställd som tar passet.
    /// </summary>
    public void Tilldela(EmployeeId anstallId)
    {
        if (Status != CoverageStatus.Open && Status != CoverageStatus.Offered)
            throw new InvalidOperationException($"Kan inte tilldela i status {Status}.");

        TilldeladAnstallId = anstallId;
        Status = CoverageStatus.Covered;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Markera passet som otäckt (ingen tog det).
    /// </summary>
    public void MarkeraOtackt()
    {
        if (Status == CoverageStatus.Covered)
            throw new InvalidOperationException("Kan inte markera täckt pass som otäckt.");

        Status = CoverageStatus.Uncovered;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum CoverageStatus
{
    Open,
    Offered,
    Covered,
    Uncovered
}
