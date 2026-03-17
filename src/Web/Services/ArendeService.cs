using RegionHR.CaseManagement.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace RegionHR.Web.Services;

public class ArendeService
{
    private readonly RegionHRDbContext _db;

    public ArendeService(RegionHRDbContext db) => _db = db;

    public async Task<List<Case>> HamtaAllaAsync(CancellationToken ct = default)
    {
        return await _db.Cases
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<List<Case>> HamtaForAnstallAsync(EmployeeId anstallId, CancellationToken ct = default)
    {
        return await _db.Cases
            .Where(c => c.AnstallId == anstallId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Case?> HamtaAsync(CaseId id, CancellationToken ct = default)
    {
        return await _db.Cases
            .Include(c => c.Godkannanden)
            .Include(c => c.Kommentarer)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<CaseId> SkapaFranvaroarendeAsync(
        EmployeeId anstallId, AbsenceType typ,
        DateOnly fran, DateOnly till, string beskrivning,
        CancellationToken ct = default)
    {
        var arende = Case.SkapaFranvaroarende(anstallId, typ, fran, till, beskrivning);
        await _db.Cases.AddAsync(arende, ct);
        await _db.SaveChangesAsync(ct);
        return arende.Id;
    }

    public async Task GodkannAsync(CaseId id, EmployeeId godkannare, string? kommentar = null, CancellationToken ct = default)
    {
        var arende = await _db.Cases
            .Include(c => c.Godkannanden)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (arende is null) return;

        arende.Godkann(godkannare, kommentar);
        arende.Avsluta();
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Case>> HamtaVantandeGodkannandenAsync(CancellationToken ct = default)
    {
        return await _db.Cases
            .Where(c => c.Status == CaseStatus.VantarGodkannande)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}
