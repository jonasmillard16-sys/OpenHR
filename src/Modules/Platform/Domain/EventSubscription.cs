namespace RegionHR.Platform.Domain;

public enum EventSubscriptionStatus
{
    Active,
    Paused,
    Failed
}

/// <summary>
/// Webhook subscription that receives domain events via HTTP POST.
/// </summary>
public sealed class EventSubscription
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string Url { get; private set; } = default!;
    public string HemligNyckel { get; private set; } = default!;
    public string EventFilter { get; private set; } = "[]";
    public EventSubscriptionStatus Status { get; private set; }
    public string RetryConfig { get; private set; } = """{"maxRetries":5,"backoffMinutes":[1,5,30,120,720]}""";
    public DateTime SkapadVid { get; private set; }
    public int KonsekutivaMisslyckanden { get; private set; }

    private EventSubscription() { }

    public static EventSubscription Skapa(
        string namn,
        string url,
        string hemligNyckel,
        string? eventFilter = null)
    {
        return new EventSubscription
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Url = url,
            HemligNyckel = hemligNyckel,
            EventFilter = eventFilter ?? "[]",
            Status = EventSubscriptionStatus.Active,
            SkapadVid = DateTime.UtcNow,
            KonsekutivaMisslyckanden = 0
        };
    }

    public void Pausa()
    {
        Status = EventSubscriptionStatus.Paused;
    }

    public void Aktivera()
    {
        Status = EventSubscriptionStatus.Active;
        KonsekutivaMisslyckanden = 0;
    }

    public void MarkeraSomFailed()
    {
        Status = EventSubscriptionStatus.Failed;
    }

    public void OkaMisslyckanden()
    {
        KonsekutivaMisslyckanden++;
        if (KonsekutivaMisslyckanden >= 10)
        {
            MarkeraSomFailed();
        }
    }

    public void AterstallMisslyckanden()
    {
        KonsekutivaMisslyckanden = 0;
    }

    /// <summary>
    /// Check if the subscription matches a given event type.
    /// Empty filter means match all events.
    /// </summary>
    public bool MatcharEventTyp(string eventTyp)
    {
        if (string.IsNullOrWhiteSpace(EventFilter) || EventFilter == "[]")
            return true;

        // Simple JSON array check — contains the event type string
        return EventFilter.Contains($"\"{eventTyp}\"", StringComparison.OrdinalIgnoreCase);
    }
}
