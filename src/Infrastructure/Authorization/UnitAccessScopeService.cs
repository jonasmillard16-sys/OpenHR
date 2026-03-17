using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Authorization;

/// <summary>
/// Provides unit-based data scoping: managers see only their unit,
/// HR admins see their division, system admins see everything.
/// </summary>
public class UnitAccessScopeService
{
    private readonly RegionHRDbContext _db;

    public UnitAccessScopeService(RegionHRDbContext db) => _db = db;

    /// <summary>
    /// Get the organization unit IDs that this user has access to.
    /// </summary>
    public async Task<List<OrganizationId>> GetAccessibleUnitsAsync(Guid userId, string role, CancellationToken ct = default)
    {
        switch (role)
        {
            case "Systemadmin":
            case "HR-admin":
                // Full access to all units
                return await _db.OrganizationUnits
                    .Select(u => u.Id)
                    .ToListAsync(ct);

            case "Chef":
                // Access to units where this user is chef
                var managedUnits = await _db.OrganizationUnits
                    .Where(u => u.ChefId == EmployeeId.From(userId))
                    .Select(u => u.Id)
                    .ToListAsync(ct);
                // Also include child units
                var allUnits = new List<OrganizationId>(managedUnits);
                foreach (var unitId in managedUnits)
                {
                    var children = await GetChildUnitsAsync(unitId, ct);
                    allUnits.AddRange(children);
                }
                return allUnits.Distinct().ToList();

            default:
                // Employees see only their own unit
                var emp = await _db.Employees
                    .Include(e => e.Anstallningar)
                    .FirstOrDefaultAsync(e => e.Id == EmployeeId.From(userId), ct);
                var activeUnit = emp?.AktivAnstallning(DateOnly.FromDateTime(DateTime.Today))?.EnhetId;
                return activeUnit.HasValue ? [activeUnit.Value] : [];
        }
    }

    private async Task<List<OrganizationId>> GetChildUnitsAsync(OrganizationId parentId, CancellationToken ct)
    {
        var children = await _db.OrganizationUnits
            .Where(u => u.OverordnadEnhetId == parentId)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var result = new List<OrganizationId>(children);
        foreach (var childId in children)
        {
            result.AddRange(await GetChildUnitsAsync(childId, ct));
        }
        return result;
    }

    /// <summary>
    /// Filter employees by accessible units.
    /// </summary>
    public IQueryable<Employee> ScopeEmployees(IQueryable<Employee> query, List<OrganizationId> accessibleUnits)
    {
        if (accessibleUnits.Count == 0) return query.Where(e => false);
        return query.Where(e => e.Anstallningar.Any(a => accessibleUnits.Contains(a.EnhetId)));
    }
}
