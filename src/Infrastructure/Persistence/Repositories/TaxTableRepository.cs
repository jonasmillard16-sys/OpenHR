using Microsoft.EntityFrameworkCore;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Persistence.Repositories;

public class TaxTableRepository : ITaxTableProvider
{
    private readonly RegionHRDbContext _db;

    public TaxTableRepository(RegionHRDbContext db) => _db = db;

    public async Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default)
    {
        return await _db.TaxTables
            .Include(t => t.Rader)
            .FirstOrDefaultAsync(t => t.Ar == year && t.Tabellnummer == tableNumber && t.Kolumn == column, ct);
    }

    public async Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default)
    {
        return await _db.TaxTables
            .Include(t => t.Rader)
            .Where(t => t.Ar == year)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<TaxTable>> GetPaginatedAsync(
        int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
    {
        var query = _db.TaxTables.AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.Ar)
            .ThenBy(t => t.Tabellnummer)
            .ThenBy(t => t.Kolumn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<TaxTable>(items, total, page, pageSize);
    }
}
