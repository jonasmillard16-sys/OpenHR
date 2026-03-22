using RegionHR.CaseManagement.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace RegionHR.Web.Services;

public class ArendeService
{
    private readonly IDbContextFactory<RegionHRDbContext> _dbFactory;

    public ArendeService(IDbContextFactory<RegionHRDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<List<Case>> HamtaAllaAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Cases
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<List<Case>> HamtaForAnstallAsync(EmployeeId anstallId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Cases
            .Where(c => c.AnstallId == anstallId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Case?> HamtaAsync(CaseId id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Cases
            .Include(c => c.Godkannanden)
            .Include(c => c.Kommentarer)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<CaseId> SkapaFranvaroarendeAsync(
        EmployeeId anstallId, AbsenceType typ,
        DateOnly fran, DateOnly till, string beskrivning,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var arende = Case.SkapaFranvaroarende(anstallId, typ, fran, till, beskrivning);
        await db.Cases.AddAsync(arende, ct);
        await db.SaveChangesAsync(ct);
        return arende.Id;
    }

    public async Task GodkannAsync(CaseId id, EmployeeId godkannare, string? kommentar = null, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var arende = await db.Cases
            .Include(c => c.Godkannanden)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (arende is null) return;

        arende.Godkann(godkannare, kommentar);
        arende.Avsluta();
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Case>> HamtaVantandeGodkannandenAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Cases
            .Where(c => c.Status == CaseStatus.VantarGodkannande)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
