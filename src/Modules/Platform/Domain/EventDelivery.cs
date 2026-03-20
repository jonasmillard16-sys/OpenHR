namespace RegionHR.Platform.Domain;

public enum EventDeliveryStatus
{
    Pending,
    Delivered,
    Failed
}

/// <summary>
/// Tracks delivery of a domain event to a webhook subscription.
/// </summary>
public sealed class EventDelivery
{
    public Guid Id { get; private set; }
    public Guid EventSubscriptionId { get; private set; }
    public Guid DomainEventRecordId { get; private set; }
    public EventDeliveryStatus Status { get; private set; }
    public int? HttpStatusKod { get; private set; }
    public int AntalForsok { get; private set; }
    public DateTime? NastaRetry { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? LeveradVid { get; private set; }

    private EventDelivery() { }

    public static EventDelivery Skapa(Guid eventSubscriptionId, Guid domainEventRecordId)
    {
        return new EventDelivery
        {
            Id = Guid.NewGuid(),
            EventSubscriptionId = eventSubscriptionId,
            DomainEventRecordId = domainEventRecordId,
            Status = EventDeliveryStatus.Pending,
            AntalForsok = 0,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void MarkeraLeverad(int httpStatus)
    {
        Status = EventDeliveryStatus.Delivered;
        HttpStatusKod = httpStatus;
        LeveradVid = DateTime.UtcNow;
        AntalForsok++;
    }

    public void MarkeraMisslyckad(int httpStatus)
    {
        Status = EventDeliveryStatus.Failed;
        HttpStatusKod = httpStatus;
        AntalForsok++;

        // Exponential backoff: 1min, 5min, 30min, 2h, 12h
        var backoffMinutes = new[] { 1, 5, 30, 120, 720 };
        var index = Math.Min(AntalForsok - 1, backoffMinutes.Length - 1);
        NastaRetry = DateTime.UtcNow.AddMinutes(backoffMinutes[index]);
    }

    public bool KanRetry(int maxRetries = 5)
    {
        return Status == EventDeliveryStatus.Failed
               && AntalForsok < maxRetries
               && NastaRetry.HasValue
               && NastaRetry.Value <= DateTime.UtcNow;
    }
}
