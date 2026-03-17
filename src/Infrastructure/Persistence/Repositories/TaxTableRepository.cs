using Microsoft.EntityFrameworkCore;
using RegionHR.Payroll.Domain;

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
}
