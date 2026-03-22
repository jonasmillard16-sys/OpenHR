using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs every 1 minute and retries failed webhook deliveries.
/// Queries EventDelivery records where KanRetry() is true and NastaRetry &lt;= now,
/// then calls WebhookDeliveryService to redeliver. Max 5 retries per delivery.
/// </summary>
public class WebhookRetryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WebhookRetryBackgroundService> _logger;
    private const int MaxRetries = 5;

    public WebhookRetryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WebhookRetryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryPendingDeliveriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebhookRetryBackgroundService: Oväntat fel vid webhook-försök");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RetryPendingDeliveriesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();
        var webhookService = scope.ServiceProvider.GetRequiredService<WebhookDeliveryService>();

        var now = DateTime.UtcNow;

        // Load failed deliveries that are eligible for retry
        var pendingRetries = await db.EventDeliveries
            .Where(d => d.Status == EventDeliveryStatus.Failed
                        && d.AntalForsok < MaxRetries
                        && d.NastaRetry.HasValue
                        && d.NastaRetry.Value <= now)
            .ToListAsync(ct);

        if (pendingRetries.Count == 0)
            return;

        _logger.LogInformation(
            "WebhookRetryBackgroundService: {Count} webhook-leveranser att försöka igen",
            pendingRetries.Count);

        var succeeded = 0;
        var failed = 0;

        foreach (var delivery in pendingRetries)
        {
            if (!delivery.KanRetry(MaxRetries))
                continue;

            try
            {
                var success = await webhookService.RedeliverAsync(delivery, ct);
                if (success)
                    succeeded++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex,
                    "WebhookRetryBackgroundService: Fel vid omleverans av delivery {DeliveryId}",
                    delivery.Id);
            }
        }

        _logger.LogInformation(
            "WebhookRetryBackgroundService: Omleveranser klara — {Succeeded} lyckades, {Failed} misslyckades",
            succeeded, failed);
    }
}
