namespace RegionHR.Notifications.Domain;

public enum NotificationType
{
    Info,
    Warning,
    Action,
    Reminder
}

public enum NotificationChannel
{
    InApp,
    Email,
    SMS,
    Push
}

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        NotificationChannel channel = NotificationChannel.InApp,
        string? actionUrl = null,
        string? relatedEntityType = null,
        string? relatedEntityId = null)
    {
        if (actionUrl?.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) == true)
            throw new ArgumentException("Ogiltig URL");

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Channel = channel,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ActionUrl = actionUrl,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
