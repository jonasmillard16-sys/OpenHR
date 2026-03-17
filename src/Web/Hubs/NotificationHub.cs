using Microsoft.AspNetCore.SignalR;

namespace RegionHR.Web.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinUserChannel(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
    }

    public static async Task SendNotification(IHubContext<NotificationHub> context, string userId, string title, string message)
    {
        await context.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", title, message);
    }
}
