namespace RegionHR.Performance.Domain;

public enum ActionItemStatus { Open, InProgress, Done }

/// <summary>
/// Åtgärdspunkt kopplad till ett 1:1-möte.
/// </summary>
public sealed class MeetingActionItem
{
    public Guid Id { get; private set; }
    public Guid MeetingId { get; private set; }
    public string Beskrivning { get; private set; } = default!;
    public Guid Ansvarig { get; private set; }
    public DateOnly? Deadline { get; private set; }
    public ActionItemStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private MeetingActionItem() { } // EF Core

    public static MeetingActionItem Skapa(Guid meetingId, string beskrivning, Guid ansvarig, DateOnly? deadline = null)
    {
        if (meetingId == Guid.Empty) throw new ArgumentException("MeetingId krävs.", nameof(meetingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(beskrivning);
        if (ansvarig == Guid.Empty) throw new ArgumentException("Ansvarig krävs.", nameof(ansvarig));

        return new MeetingActionItem
        {
            Id = Guid.NewGuid(),
            MeetingId = meetingId,
            Beskrivning = beskrivning,
            Ansvarig = ansvarig,
            Deadline = deadline,
            Status = ActionItemStatus.Open,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Påbörjar arbetet med åtgärdspunkten.
    /// </summary>
    public void Paborja()
    {
        if (Status != ActionItemStatus.Open)
            throw new InvalidOperationException($"Kan bara påbörja öppna åtgärder. Nuvarande status: {Status}.");

        Status = ActionItemStatus.InProgress;
    }

    /// <summary>
    /// Slutför åtgärdspunkten.
    /// </summary>
    public void Slutfor()
    {
        if (Status == ActionItemStatus.Done)
            throw new InvalidOperationException("Åtgärden är redan slutförd.");

        Status = ActionItemStatus.Done;
    }
}
