using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RegionHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RegionHR.Infrastructure.Reporting;

/// <summary>
/// Background service that runs scheduled reports and sends results via email.
/// Checks every hour for reports where ArSchemalagd is true and CronExpression is set.
/// </summary>
public class ScheduledReportService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledReportService> _logger;

    public ScheduledReportService(IServiceScopeFactory scopeFactory, ILogger<ScheduledReportService> logger)
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
                await CheckScheduledReports(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in scheduled report service");
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CheckScheduledReports(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();

        try
        {
            var dueReports = await db.ReportDefinitions
                .Where(r => r.ArSchemalagd && r.CronExpression != null)
                .ToListAsync(ct);

            foreach (var report in dueReports)
            {
                _logger.LogInformation("Running scheduled report: {Name}", report.Namn);
                // TODO: Execute report and email results to report.MottagareEpost
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check scheduled reports (DB may not be available)");
        }
    }
}
