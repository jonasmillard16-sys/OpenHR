namespace RegionHR.Notifications.Domain;

/// <summary>
/// Represents a Web Push subscription for an employee's browser/device.
/// Used to deliver push notifications via the Web Push protocol (RFC 8030).
/// </summary>
public sealed class PushSubscription
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256dhKey { get; private set; } = string.Empty;
    public string AuthKey { get; private set; } = string.Empty;
    public DateTime SkapadVid { get; private set; }
    public bool ArAktiv { get; private set; }

    private PushSubscription() { }

    public static PushSubscription Registrera(Guid anstallId, string endpoint, string p256dhKey, string authKey)
    {
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(p256dhKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(authKey);

        return new PushSubscription
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Endpoint = endpoint,
            P256dhKey = p256dhKey,
            AuthKey = authKey,
            SkapadVid = DateTime.UtcNow,
            ArAktiv = true
        };
    }

    public void Avaktivera()
    {
        ArAktiv = false;
    }

    public void Aktivera()
    {
        ArAktiv = true;
    }
}
