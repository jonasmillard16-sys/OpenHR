using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Bud på ett öppet pass. Anställd anger prioritet (1 = högst) och valfri motivering.
/// </summary>
public sealed class ShiftBid
{
    public Guid Id { get; private set; }
    public Guid OpenShiftId { get; private set; }
    public EmployeeId AnstallId { get; private set; }

    /// <summary>Prioritet: 1 = högst, 2 = näst högst, etc.</summary>
    public int Prioritet { get; private set; }

    public string? Motivering { get; private set; }
    public ShiftBidStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private ShiftBid() { }

    /// <summary>Skapa ett nytt bud på ett öppet pass.</summary>
    public static ShiftBid Skapa(
        Guid openShiftId,
        EmployeeId anstallId,
        int prioritet = 1,
        string? motivering = null)
    {
        if (prioritet < 1)
            throw new ArgumentException("Prioritet måste vara minst 1.", nameof(prioritet));

        return new ShiftBid
        {
            Id = Guid.NewGuid(),
            OpenShiftId = openShiftId,
            AnstallId = anstallId,
            Prioritet = prioritet,
            Motivering = motivering,
            Status = ShiftBidStatus.Pending,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>Acceptera budet.</summary>
    public void Acceptera()
    {
        if (Status != ShiftBidStatus.Pending)
            throw new InvalidOperationException($"Kan inte acceptera bud i status {Status}.");
        Status = ShiftBidStatus.Accepted;
    }

    /// <summary>Avvisa budet.</summary>
    public void Avvisa()
    {
        if (Status != ShiftBidStatus.Pending)
            throw new InvalidOperationException($"Kan inte avvisa bud i status {Status}.");
        Status = ShiftBidStatus.Rejected;
    }
}

public enum ShiftBidStatus
{
    Pending,
    Accepted,
    Rejected
}
