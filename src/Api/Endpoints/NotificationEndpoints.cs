using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Notifications.Domain;

namespace RegionHR.Api.Endpoints;

public static class NotificationEndpoints
{
    public static WebApplication MapNotificationEndpoints(this WebApplication app)
    {
        var notiser = app.MapGroup("/api/v1/notiser").WithTags("Notiser").RequireAuthorization();

        // ============================================================
        // Lista notiser för användare
        // ============================================================

        notiser.MapGet("/", async (Guid userId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var notifications = await db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(notifications);
        }).WithName("ListNotifications");

        // ============================================================
        // Räkna olästa notiser
        // ============================================================

        notiser.MapGet("/olasta", async (Guid userId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var count = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync(ct);

            return Results.Ok(new { AntalOlasta = count });
        }).WithName("CountUnreadNotifications");

        // ============================================================
        // Markera en notis som läst
        // ============================================================

        notiser.MapPost("/{id:guid}/las", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
            if (notification is null) return Results.NotFound();

            notification.MarkAsRead();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { notification.Id, notification.IsRead, notification.ReadAt });
        }).WithName("MarkNotificationAsRead");

        // ============================================================
        // Markera alla notiser som lästa för en användare
        // ============================================================

        notiser.MapPost("/las-alla", async (Guid userId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var unread = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(ct);

            foreach (var n in unread)
                n.MarkAsRead();

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { MarkeradeAntal = unread.Count });
        }).WithName("MarkAllNotificationsAsRead");

        // ============================================================
        // Skapa notis
        // ============================================================

        notiser.MapPost("/", async (CreateNotificationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var notification = Notification.Create(
                req.UserId,
                req.Title,
                req.Message,
                NotificationType.Info);

            await db.Notifications.AddAsync(notification, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/notiser/{notification.Id}", new
            {
                notification.Id,
                notification.UserId,
                notification.Title,
                notification.CreatedAt
            });
        }).WithName("CreateNotification");

        // ============================================================
        // Push-prenumeration — registrera browser push subscription
        // ============================================================

        notiser.MapPost("/push-subscription", async (RegisterPushSubscriptionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            // Deactivate any existing subscriptions for same endpoint
            var existing = await db.PushSubscriptions
                .Where(p => p.AnstallId == req.AnstallId && p.Endpoint == req.Endpoint)
                .ToListAsync(ct);

            foreach (var old in existing)
                old.Avaktivera();

            var subscription = PushSubscription.Registrera(
                req.AnstallId,
                req.Endpoint,
                req.P256dhKey,
                req.AuthKey);

            await db.PushSubscriptions.AddAsync(subscription, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/notiser/push-subscription/{subscription.Id}", new
            {
                subscription.Id,
                subscription.AnstallId,
                subscription.SkapadVid,
                subscription.ArAktiv
            });
        }).WithName("RegisterPushSubscription");

        // ============================================================
        // Ta bort push-prenumeration
        // ============================================================

        notiser.MapDelete("/push-subscription/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var subscription = await db.PushSubscriptions.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (subscription is null) return Results.NotFound();

            subscription.Avaktivera();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { subscription.Id, subscription.ArAktiv });
        }).WithName("DeactivatePushSubscription");

        return app;
    }
}

// Request DTOs
record CreateNotificationRequest(Guid UserId, string Title, string Message);
record RegisterPushSubscriptionRequest(Guid AnstallId, string Endpoint, string P256dhKey, string AuthKey);
