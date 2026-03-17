using RegionHR.Notifications.Domain;
using Xunit;

namespace RegionHR.Notifications.Tests;

public class NotificationTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var title = "Test Title";
        var message = "Test Message";
        var type = NotificationType.Warning;
        var channel = NotificationChannel.Email;
        var actionUrl = "https://example.com/action";
        var relatedEntityType = "LeaveRequest";
        var relatedEntityId = Guid.NewGuid().ToString();

        var notification = Notification.Create(
            userId, title, message, type, channel,
            actionUrl, relatedEntityType, relatedEntityId);

        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(userId, notification.UserId);
        Assert.Equal(title, notification.Title);
        Assert.Equal(message, notification.Message);
        Assert.Equal(type, notification.Type);
        Assert.Equal(channel, notification.Channel);
        Assert.False(notification.IsRead);
        Assert.True(notification.CreatedAt <= DateTime.UtcNow);
        Assert.Null(notification.ReadAt);
        Assert.Equal(actionUrl, notification.ActionUrl);
        Assert.Equal(relatedEntityType, notification.RelatedEntityType);
        Assert.Equal(relatedEntityId, notification.RelatedEntityId);
    }

    [Fact]
    public void MarkAsRead_SetsIsReadAndReadAt()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Title", "Message", NotificationType.Info);

        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);

        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        Assert.True(notification.ReadAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsRead_AlreadyRead_UpdatesReadAt()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Title", "Message", NotificationType.Info);

        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;

        // Small delay to ensure time difference
        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        Assert.True(notification.ReadAt >= firstReadAt);
    }

    [Fact]
    public void NotificationTemplate_Create_SetsKeyCorrectly()
    {
        var key = "leave-approved";
        var titleTemplate = "Leave Approved";
        var messageTemplate = "Your leave request for {dates} has been approved.";
        var defaultType = NotificationType.Info;
        var defaultChannel = NotificationChannel.InApp;

        var template = NotificationTemplate.Create(
            key, titleTemplate, messageTemplate, defaultType, defaultChannel);

        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal(key, template.TemplateKey);
        Assert.Equal(titleTemplate, template.TitleTemplate);
        Assert.Equal(messageTemplate, template.MessageTemplate);
        Assert.Equal(defaultType, template.DefaultType);
        Assert.Equal(defaultChannel, template.DefaultChannel);
    }
}
