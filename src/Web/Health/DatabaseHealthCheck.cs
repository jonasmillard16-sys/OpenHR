using Microsoft.Extensions.Diagnostics.HealthChecks;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Web.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseHealthCheck(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RegionHRDbContext>();
            await db.Database.CanConnectAsync(ct);
            return HealthCheckResult.Healthy("PostgreSQL OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL unavailable", ex);
        }
    }
}
