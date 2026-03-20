namespace RegionHR.Notifications.Domain;

public sealed class NotificationPreference
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string NotisTyp { get; private set; } = default!;
    public bool InApp { get; private set; }
    public bool Epost { get; private set; }
    public DateTime UppdateradVid { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference Skapa(Guid anstallId, string notisTyp, bool inApp = true, bool epost = true)
    {
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));
        ArgumentException.ThrowIfNullOrWhiteSpace(notisTyp);
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            NotisTyp = notisTyp,
            InApp = inApp,
            Epost = epost,
            UppdateradVid = DateTime.UtcNow
        };
    }

    public void Uppdatera(bool inApp, bool epost)
    {
        InApp = inApp;
        Epost = epost;
        UppdateradVid = DateTime.UtcNow;
    }
}
