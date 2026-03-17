using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RegionHR.Core.Contracts;
using RegionHR.Core.Domain;
using RegionHR.Payroll.Domain;
using RegionHR.Payroll.Engine;
using RegionHR.Scheduling.Optimization;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Persistence.Repositories;
using RegionHR.Infrastructure.Authorization;
using RegionHR.Infrastructure.BackgroundJobs;
using RegionHR.Infrastructure.Export;
using RegionHR.Infrastructure.Storage;
using RegionHR.Infrastructure.GDPR;
using RegionHR.Infrastructure.Notifications;
using RegionHR.Infrastructure.Documents;
using RegionHR.Infrastructure.Reporting;
using RegionHR.Infrastructure.Payroll;
using RegionHR.Infrastructure.Scheduling;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace RegionHR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, bool useInMemory = false)
    {
        // Audit interceptor
        var auditInterceptor = new AuditInterceptor();

        // DbContext
        if (useInMemory)
        {
            services.AddDbContext<RegionHRDbContext>(options =>
                options.UseInMemoryDatabase("RegionHR-Dev")
                    .AddInterceptors(auditInterceptor));
        }
        else
        {
            services.AddDbContext<RegionHRDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                })
                .AddInterceptors(auditInterceptor));
        }

        // Repositories
        services.AddScoped<IRepository<Employee, EmployeeId>, EmployeeRepository>();
        services.AddScoped<IRepository<PayrollRun, PayrollRunId>, PayrollRunRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<PayrollRunRepository>();

        // Module contracts
        services.AddScoped<ICoreHRModule, CoreHRModuleService>();

        // Payroll services
        services.AddScoped<ITaxTableProvider, TaxTableRepository>();
        services.AddScoped<ICollectiveAgreementRulesEngine, CollectiveAgreementRulesEngine>();
        services.AddScoped<PayrollCalculationEngine>();

        // Scheduling services
        services.AddSingleton<ConstraintScheduleSolver>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<RegionHRDbContext>()));

        // Export services
        services.AddSingleton<ExportService>();
        services.AddSingleton<PdfPayslipGenerator>();

        // Authorization services
        services.AddScoped<UnitScopeService>();
        services.AddScoped<UnitAccessScopeService>();

        // Notifications
        services.AddSingleton<EmailNotificationSender>();
        services.AddSingleton<SmsNotificationSender>();

        // File storage
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService());

        // GDPR
        services.AddScoped<RegisterutdragGenerator>();

        // Reporting
        services.AddScoped<ReportGenerator>();

        // Document template engine & e-signing
        services.AddSingleton<DocumentTemplateEngine>();
        services.AddSingleton<ISigningService, SimpleConfirmationSigningService>();

        // Swedish payroll engine
        services.AddSingleton<SwedishTaxCalculator>();
        services.AddSingleton<KollektivavtalEngine>();

        // Schema optimization
        services.AddSingleton<SchemaOptimizer>();

        // Background services
        services.AddHostedService<NotificationReminderService>();
        services.AddHostedService<RetentionCleanupService>();
        services.AddHostedService<ScheduledReportService>();

        // OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("RegionHR"))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation())
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation());

        return services;
    }
}

internal class UnitOfWork : IUnitOfWork
{
    private readonly RegionHRDbContext _db;
    public UnitOfWork(RegionHRDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
