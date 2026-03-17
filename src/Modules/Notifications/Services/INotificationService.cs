using RegionHR.Notifications.Domain;

namespace RegionHR.Notifications.Services;

public interface INotificationService
{
    Task SendAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationChannel channel = NotificationChannel.InApp,
        string? actionUrl = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Notification>> GetUnreadAsync(Guid userId, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
