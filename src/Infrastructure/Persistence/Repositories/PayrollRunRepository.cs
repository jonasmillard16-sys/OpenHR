using Microsoft.EntityFrameworkCore;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Repositories;

public class PayrollRunRepository : IRepository<PayrollRun, PayrollRunId>
{
    private readonly RegionHRDbContext _db;

    public PayrollRunRepository(RegionHRDbContext db) => _db = db;

    public async Task<PayrollRun?> GetByIdAsync(PayrollRunId id, CancellationToken ct = default)
    {
        return await _db.PayrollRuns
            .Include(r => r.Resultat)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<PayrollRun>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.PayrollRuns.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToListAsync(ct);
    }

    public async Task AddAsync(PayrollRun entity, CancellationToken ct = default)
    {
        await _db.PayrollRuns.AddAsync(entity, ct);
    }

    public Task UpdateAsync(PayrollRun entity, CancellationToken ct = default)
    {
        _db.PayrollRuns.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(PayrollRunId id, CancellationToken ct = default)
    {
        var entity = await _db.PayrollRuns.FindAsync([id], ct);
        if (entity is not null)
            _db.PayrollRuns.Remove(entity);
    }

    public async Task<PayrollRun?> GetByPeriodAsync(int year, int month, CancellationToken ct = default)
    {
        return await _db.PayrollRuns
            .Include(r => r.Resultat)
            .FirstOrDefaultAsync(r => r.Year == year && r.Month == month, ct);
    }
}
