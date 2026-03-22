using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Payroll.Engine;

namespace RegionHR.Infrastructure.Payroll;

/// <summary>
/// Reads system settings (e.g. IBB, PBB) from the SystemSetting table.
/// Implements the ISystemSettingProvider interface defined in the Payroll module.
/// </summary>
public class DbSystemSettingProvider : ISystemSettingProvider
{
    private readonly IDbContextFactory<RegionHRDbContext> _dbFactory;

    public DbSystemSettingProvider(IDbContextFactory<RegionHRDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<decimal?> GetDecimalAsync(string key, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var setting = await db.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Nyckel == key, ct);

        return setting?.HamtaDecimal();
    }
}
