using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs once per day to anonymize expired retention records.
/// Finds RetentionRecords where RetentionExpires has passed and IsAnonymized is false,
/// then calls Anonymize() on each.
/// </summary>
public class RetentionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetentionCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);

    public RetentionCleanupService(IServiceScopeFactory scopeFactory, ILogger<RetentionCleanupService> logger)
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
                _logger.LogInformation("RetentionCleanupService: Starting retention cleanup");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

                var expired = await db.RetentionRecords
                    .Where(r => r.RetentionExpires < DateTime.UtcNow && !r.IsAnonymized)
                    .ToListAsync(stoppingToken);

                foreach (var record in expired)
                {
                    record.Anonymize();
                }

                var count = expired.Count;

                if (count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("RetentionCleanupService: Anonymized {Count} expired retention records", count);
                }
                else
                {
                    _logger.LogInformation("RetentionCleanupService: No expired retention records found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retention cleanup service");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
