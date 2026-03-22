using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IRepository<Employee, EmployeeId>
{
    private readonly RegionHRDbContext _db;

    public EmployeeRepository(RegionHRDbContext db) => _db = db;

    public async Task<Employee?> GetByIdAsync(EmployeeId id, CancellationToken ct = default)
    {
        return await _db.Employees
            .Include(e => e.Anstallningar)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Employees.ToListAsync(ct);
    }

    public async Task<PaginatedResult<Employee>> GetPaginatedAsync(
        int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = _db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(e =>
                e.Fornamn.Contains(searchTerm) ||
                e.Efternamn.Contains(searchTerm));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Efternamn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Employee>(items, total, page, pageSize);
    }

    public async Task AddAsync(Employee entity, CancellationToken ct = default)
    {
        await _db.Employees.AddAsync(entity, ct);
    }

    public Task UpdateAsync(Employee entity, CancellationToken ct = default)
    {
        _db.Employees.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(EmployeeId id, CancellationToken ct = default)
    {
        var entity = await _db.Employees.FindAsync([id], ct);
        if (entity is not null)
            _db.Employees.Remove(entity);
    }

    public async Task<IReadOnlyList<Employee>> GetByUnitAsync(OrganizationId unitId, DateOnly date, CancellationToken ct = default)
    {
        return await _db.Employees
            .Include(e => e.Anstallningar)
            .Where(e => e.Anstallningar.Any(a =>
                a.EnhetId == unitId &&
                a.Giltighetsperiod.Start <= date &&
                (a.Giltighetsperiod.End == null || a.Giltighetsperiod.End >= date)))
            .ToListAsync(ct);
    }
}
