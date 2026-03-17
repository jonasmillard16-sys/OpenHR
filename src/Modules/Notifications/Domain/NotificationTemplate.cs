namespace RegionHR.Notifications.Domain;

public class NotificationTemplate
{
    public Guid Id { get; private set; }
    public string TemplateKey { get; private set; } = string.Empty;
    public string TitleTemplate { get; private set; } = string.Empty;
    public string MessageTemplate { get; private set; } = string.Empty;
    public NotificationType DefaultType { get; private set; }
    public NotificationChannel DefaultChannel { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        string key,
        string titleTemplate,
        string messageTemplate,
        NotificationType defaultType,
        NotificationChannel defaultChannel)
    {
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = key,
            TitleTemplate = titleTemplate,
            MessageTemplate = messageTemplate,
            DefaultType = defaultType,
            DefaultChannel = defaultChannel
        };
    }
}
