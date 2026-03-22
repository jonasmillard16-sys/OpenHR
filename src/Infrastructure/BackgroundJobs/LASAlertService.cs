using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Notifications.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs every 12 hours and creates alert notifications
/// for employees approaching LAS accumulation thresholds: 300, 330, 350, and 360 days.
/// </summary>
public class LASAlertService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LASAlertService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);

    // Thresholds in ascending order; each triggers a separate alert type
    private static readonly (int Days, string Label)[] Thresholds =
    [
        (300, "300"),
        (330, "330"),
        (350, "350"),
        (360, "360"),
    ];

    public LASAlertService(IServiceScopeFactory scopeFactory, ILogger<LASAlertService> logger)
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
                _logger.LogInformation("LASAlertService: Checking LAS thresholds");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

                await CheckLASThresholds(db, stoppingToken);

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("LASAlertService: LAS threshold checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking LAS thresholds");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    internal async Task CheckLASThresholds(RegionHRDbContext db, CancellationToken ct)
    {
        // Avoid duplicate notifications within 24 hours
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var nearLimit = await db.LASAccumulations
            .Where(l => l.AckumuleradeDagar >= 300)
            .ToListAsync(ct);

        foreach (var las in nearLimit)
        {
            var entityId = las.Id.ToString();

            // Find the highest threshold crossed and notify for each new threshold
            foreach (var (thresholdDays, label) in Thresholds)
            {
                if (las.AckumuleradeDagar < thresholdDays)
                    break;

                var relatedEntityType = $"LASAccumulation-{label}d";

                bool alreadyNotified = await db.Notifications
                    .AnyAsync(n =>
                        n.RelatedEntityType == relatedEntityType &&
                        n.RelatedEntityId == entityId &&
                        n.CreatedAt > cutoff, ct);

                if (!alreadyNotified)
                {
                    var notifType = thresholdDays >= 350
                        ? NotificationType.Warning
                        : NotificationType.Reminder;

                    var notification = Notification.Create(
                        las.AnstallId.Value,
                        $"LAS-varning: {label} dagar uppnadda",
                        $"LAS-ackumulering har natt {label} dagar ({las.AckumuleradeDagar} totalt av max 365). Atgard kravs for att undvika konvertering till tillsvidareanstallning.",
                        notifType,
                        NotificationChannel.InApp,
                        relatedEntityType: relatedEntityType,
                        relatedEntityId: entityId);

                    db.Notifications.Add(notification);
                }
            }
        }
    }
}
