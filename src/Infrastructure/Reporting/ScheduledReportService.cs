using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Notifications;
using RegionHR.Notifications.Domain;
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
        var reportGenerator = scope.ServiceProvider.GetRequiredService<ReportGenerator>();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailNotificationSender>();

        try
        {
            var dueReports = await db.ReportDefinitions
                .Where(r => r.ArSchemalagd && r.CronExpression != null)
                .ToListAsync(ct);

            foreach (var report in dueReports)
            {
                _logger.LogInformation("Running scheduled report: {Name}", report.Namn);

                try
                {
                    // Generate the report bytes
                    var reportBytes = await reportGenerator.GenerateAsync(report.Typ, ct);
                    _logger.LogInformation(
                        "Scheduled report '{Name}' generated successfully ({Bytes} bytes)",
                        report.Namn, reportBytes.Length);

                    // Send email notification to recipient (without binary attachment —
                    // EmailNotificationSender does not support attachments; the report
                    // is available in the OpenHR reports section).
                    if (!string.IsNullOrWhiteSpace(report.MottagareEpost))
                    {
                        await emailSender.SendAsync(
                            report.MottagareEpost,
                            report.MottagareEpost,
                            $"Schemalagd rapport klar: {report.Namn}",
                            $"<p>Den schemalagda rapporten <strong>{report.Namn}</strong> har genererats.</p>" +
                            $"<p>Logga in i OpenHR och navigera till <em>Rapporter</em> för att ladda ned rapporten.</p>",
                            ct);

                        // Also create an InApp notification if the email address belongs to an employee
                        var matchingEmployee = await db.Employees
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Epost == report.MottagareEpost, ct);

                        if (matchingEmployee is not null)
                        {
                            var notification = Notification.Create(
                                matchingEmployee.Id.Value,
                                $"Rapport klar: {report.Namn}",
                                $"Den schemalagda rapporten '{report.Namn}' är genererad och finns i Rapporter.",
                                NotificationType.Info,
                                NotificationChannel.InApp,
                                actionUrl: "/rapporter",
                                relatedEntityType: "ReportDefinition",
                                relatedEntityId: report.Id.ToString());

                            db.Notifications.Add(notification);
                            await db.SaveChangesAsync(ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute scheduled report '{Name}'", report.Namn);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check scheduled reports (DB may not be available)");
        }
    }
}
