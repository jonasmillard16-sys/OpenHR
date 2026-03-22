using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Notifications.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs every 24 hours and creates reminder notifications
/// for certifications expiring within 30, 60, or 90 days.
/// </summary>
public class CertificationReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CertificationReminderService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    private static readonly int[] ThresholdDays = [90, 60, 30];

    public CertificationReminderService(IServiceScopeFactory scopeFactory, ILogger<CertificationReminderService> logger)
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
                _logger.LogInformation("CertificationReminderService: Checking expiring certifications");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

                await CheckExpiringCertifications(db, stoppingToken);

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("CertificationReminderService: Certification checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking certifications");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    internal async Task CheckExpiringCertifications(RegionHRDbContext db, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var ninetyDaysFromNow = today.AddDays(90);
        // Avoid re-notifying within 7 days for the same cert+threshold
        var weekCutoff = DateTime.UtcNow.AddDays(-7);

        var expiring = await db.Certifications
            .Where(c => c.GiltigTill != null &&
                        c.GiltigTill >= today &&
                        c.GiltigTill <= ninetyDaysFromNow)
            .ToListAsync(ct);

        foreach (var cert in expiring)
        {
            var daysLeft = cert.GiltigTill!.Value.DayNumber - today.DayNumber;
            var entityId = cert.Id.ToString();

            // Determine which threshold bracket this certification falls into
            // and create one notification per relevant threshold
            foreach (var threshold in ThresholdDays)
            {
                if (daysLeft > threshold)
                    continue;

                var relatedEntityType = $"Certification-Expiring-{threshold}d";

                bool alreadyNotified = await db.Notifications
                    .AnyAsync(n =>
                        n.RelatedEntityType == relatedEntityType &&
                        n.RelatedEntityId == entityId &&
                        n.CreatedAt > weekCutoff, ct);

                if (!alreadyNotified)
                {
                    var notifType = threshold <= 30
                        ? NotificationType.Warning
                        : NotificationType.Reminder;

                    var notification = Notification.Create(
                        cert.AnstallId,
                        $"Certifiering gar ut inom {threshold} dagar",
                        $"Certifiering '{cert.Namn}' gar ut om {daysLeft} dagar ({cert.GiltigTill.Value:yyyy-MM-dd}). Fornya i tid.",
                        notifType,
                        NotificationChannel.InApp,
                        relatedEntityType: relatedEntityType,
                        relatedEntityId: entityId);

                    db.Notifications.Add(notification);
                }

                // Only notify for the most urgent threshold that applies
                break;
            }
        }
    }
}
