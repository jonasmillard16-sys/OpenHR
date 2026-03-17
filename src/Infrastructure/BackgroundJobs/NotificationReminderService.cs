using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Notifications.Domain;
using RegionHR.GDPR.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs every hour and creates automatic notification reminders
/// for sick leave deadlines, expiring certifications, LAS warnings, GDPR deadlines,
/// and retention expiry.
/// </summary>
public class NotificationReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationReminderService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public NotificationReminderService(IServiceScopeFactory scopeFactory, ILogger<NotificationReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("NotificationReminderService: Starting reminder checks");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

                await CheckSickLeaveReminders(db, stoppingToken);
                await CheckExpiringCertifications(db, stoppingToken);
                await CheckLASWarnings(db, stoppingToken);
                await CheckGDPRDeadlines(db, stoppingToken);
                await CheckRetentionExpiry(db, stoppingToken);

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("NotificationReminderService: Reminder checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification reminder service");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    /// <summary>
    /// Checks sick leave notifications where doctor's certificate is required (day >= 7)
    /// but not submitted, or FK reporting is required (day >= 14) but not done.
    /// </summary>
    internal async Task CheckSickLeaveReminders(RegionHRDbContext db, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        // Lakarintyg required (day >= 8 in domain, but spec says >= 7 for notification)
        var needsCertificate = await db.SickLeaveNotifications
            .Where(s => s.SjukDag >= 7 && !s.LakarintygInlamnat)
            .ToListAsync(ct);

        foreach (var sick in needsCertificate)
        {
            var entityId = sick.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "SickLeaveNotification-Lakarintyg" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > cutoff, ct);

            if (!alreadyNotified)
            {
                var notification = Notification.Create(
                    sick.AnstallId,
                    "Lakarintyg kravs",
                    $"Sjukdag {sick.SjukDag}: Lakarintyg maste lamnas in. Startdatum: {sick.StartDatum}",
                    NotificationType.Warning,
                    NotificationChannel.InApp,
                    relatedEntityType: "SickLeaveNotification-Lakarintyg",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }

        // FK anmalan required (day >= 14)
        var needsFKReport = await db.SickLeaveNotifications
            .Where(s => s.SjukDag >= 14 && !s.FKAnmalanGjord)
            .ToListAsync(ct);

        foreach (var sick in needsFKReport)
        {
            var entityId = sick.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "SickLeaveNotification-FK" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > cutoff, ct);

            if (!alreadyNotified)
            {
                var notification = Notification.Create(
                    sick.AnstallId,
                    "Forsakringskassan-anmalan kravs",
                    $"Sjukdag {sick.SjukDag}: Anmalan till Forsakringskassan maste goras. Startdatum: {sick.StartDatum}",
                    NotificationType.Warning,
                    NotificationChannel.InApp,
                    relatedEntityType: "SickLeaveNotification-FK",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }
    }

    /// <summary>
    /// Checks certifications expiring within 90 days and creates warning notifications.
    /// Skips if a notification was already sent this week.
    /// </summary>
    internal async Task CheckExpiringCertifications(RegionHRDbContext db, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ninetyDaysFromNow = today.AddDays(90);
        var weekCutoff = DateTime.UtcNow.AddDays(-7);

        var expiring = await db.Certifications
            .Where(c => c.GiltigTill != null &&
                        c.GiltigTill >= today &&
                        c.GiltigTill <= ninetyDaysFromNow)
            .ToListAsync(ct);

        foreach (var cert in expiring)
        {
            var entityId = cert.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "Certification-Expiring" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > weekCutoff, ct);

            if (!alreadyNotified)
            {
                var daysLeft = cert.GiltigTill!.Value.DayNumber - today.DayNumber;
                var notification = Notification.Create(
                    cert.AnstallId,
                    "Certifiering gar ut snart",
                    $"Certifiering '{cert.Namn}' gar ut om {daysLeft} dagar ({cert.GiltigTill.Value}).",
                    NotificationType.Warning,
                    NotificationChannel.InApp,
                    relatedEntityType: "Certification-Expiring",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }
    }

    /// <summary>
    /// Checks LAS accumulations nearing the 365-day SAVA limit (>= 300 days).
    /// </summary>
    internal async Task CheckLASWarnings(RegionHRDbContext db, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var nearLimit = await db.LASAccumulations
            .Where(l => l.AckumuleradeDagar >= 300)
            .ToListAsync(ct);

        foreach (var las in nearLimit)
        {
            var entityId = las.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "LASAccumulation-Warning" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > cutoff, ct);

            if (!alreadyNotified)
            {
                var notification = Notification.Create(
                    las.AnstallId.Value,
                    "LAS-varning: Narmar sig grans",
                    $"LAS-ackumulering: {las.AckumuleradeDagar} dagar av max 365 (SAVA). Atgard kravs.",
                    NotificationType.Warning,
                    NotificationChannel.InApp,
                    relatedEntityType: "LASAccumulation-Warning",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }
    }

    /// <summary>
    /// Checks GDPR data subject requests that are not completed and have deadlines within 7 days.
    /// </summary>
    internal async Task CheckGDPRDeadlines(RegionHRDbContext db, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);

        var approaching = await db.DataSubjectRequests
            .Where(r => r.Status != RequestStatus.Klar &&
                        r.Deadline <= sevenDaysFromNow &&
                        r.Deadline >= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var req in approaching)
        {
            var entityId = req.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "DataSubjectRequest-Deadline" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > cutoff, ct);

            if (!alreadyNotified)
            {
                var daysLeft = (req.Deadline - DateTime.UtcNow).Days;
                // Notify the handler if assigned, otherwise the subject
                var targetUserId = req.HandlaggarId != null
                    ? Guid.TryParse(req.HandlaggarId, out var handlaggarGuid) ? handlaggarGuid : req.AnstallId
                    : req.AnstallId;

                var notification = Notification.Create(
                    targetUserId,
                    "GDPR-deadline narmar sig",
                    $"GDPR-begaran ({req.Typ}) har deadline om {daysLeft} dagar ({req.Deadline:yyyy-MM-dd}). Status: {req.Status}.",
                    NotificationType.Warning,
                    NotificationChannel.InApp,
                    relatedEntityType: "DataSubjectRequest-Deadline",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }
    }

    /// <summary>
    /// Checks retention records that have expired but have not been anonymized.
    /// Creates a notification to system admin.
    /// </summary>
    internal async Task CheckRetentionExpiry(RegionHRDbContext db, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var expired = await db.RetentionRecords
            .Where(r => r.RetentionExpires < DateTime.UtcNow && !r.IsAnonymized)
            .ToListAsync(ct);

        foreach (var record in expired)
        {
            var entityId = record.Id.ToString();

            bool alreadyNotified = await db.Notifications
                .AnyAsync(n =>
                    n.RelatedEntityType == "RetentionRecord-Expired" &&
                    n.RelatedEntityId == entityId &&
                    n.CreatedAt > cutoff, ct);

            if (!alreadyNotified)
            {
                // Use a well-known system admin GUID (Guid.Empty) as the target
                var notification = Notification.Create(
                    Guid.Empty,
                    "Gallringsperiod har gatt ut",
                    $"Gallringspost for {record.EntityType} (ID: {record.EntityId}) har passerat gallringsdatum ({record.RetentionExpires:yyyy-MM-dd}) och behover anonymiseras.",
                    NotificationType.Reminder,
                    NotificationChannel.InApp,
                    relatedEntityType: "RetentionRecord-Expired",
                    relatedEntityId: entityId);

                db.Notifications.Add(notification);
            }
        }
    }
}
