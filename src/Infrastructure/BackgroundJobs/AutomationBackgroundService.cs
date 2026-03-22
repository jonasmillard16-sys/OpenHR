using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegionHR.Automation.Domain;

namespace RegionHR.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that runs every 5 minutes and fires scheduled automation rules
/// by calling EvaluateCronAsync for each known cron category.
/// Uses IServiceScopeFactory to resolve scoped services safely.
/// </summary>
public class AutomationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationBackgroundService> _logger;

    /// <summary>
    /// Known cron trigger categories that are evaluated on every tick.
    /// These match TriggerTyp values on seeded AutomationRules.
    /// </summary>
    private static readonly string[] CronCategories =
    [
        "Cron.Daily",
        "Cron.Weekly",
        "Cron.Monthly",
        "Cron.SickLeave",
        "Cron.LAS",
        "Cron.Certification",
        "Cron.Performance",
        "Cron.Onboarding",
        "Cron.Offboarding",
        "Cron.Compliance"
    ];

    public AutomationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutomationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunScheduledRulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutomationBackgroundService: Oväntat fel vid körning av schemalagda regler");
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

    private async Task RunScheduledRulesAsync(CancellationToken ct)
    {
        _logger.LogDebug("AutomationBackgroundService: Utvärderar schemalagda automationsregler");

        using var scope = _scopeFactory.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<IAutomationEngine>();

        foreach (var category in CronCategories)
        {
            try
            {
                await engine.EvaluateCronAsync(category, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AutomationBackgroundService: Fel vid evaluering av kategori '{Category}'",
                    category);
            }
        }

        _logger.LogDebug("AutomationBackgroundService: Schemalagda regler utvärderade");
    }
}
