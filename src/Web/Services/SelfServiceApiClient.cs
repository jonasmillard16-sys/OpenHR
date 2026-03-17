using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Web.Services;

public class SelfServiceApiClient
{
    private readonly RegionHRDbContext _db;
    public SelfServiceApiClient(RegionHRDbContext db) => _db = db;

    public async Task<DashboardData> GetDashboardAsync(EmployeeId id, CancellationToken ct = default)
    {
        var emp = await _db.Employees
            .Include(e => e.Anstallningar)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        var year = DateTime.Today.Year;
        var vacBalance = await _db.VacationBalances
            .FirstOrDefaultAsync(b => b.AnstallId == id.Value && b.Ar == year, ct);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekEnd = today.AddDays(7);
        var shifts = await _db.ScheduledShifts
            .Where(s => s.AnstallId == id && s.Datum >= today && s.Datum <= weekEnd)
            .OrderBy(s => s.Datum)
            .Take(5)
            .ToListAsync(ct);

        var openCases = await _db.Cases
            .CountAsync(c => c.AnstallId == id && c.Status != CaseStatus.Avslutad, ct);

        var unreadNotifs = await _db.Notifications
            .CountAsync(n => n.UserId == id.Value && !n.IsRead, ct);

        return new DashboardData
        {
            Fornamn = emp?.Fornamn ?? "",
            Efternamn = emp?.Efternamn ?? "",
            SemesterdagarKvar = vacBalance?.TillgangligaDagar ?? 0,
            OppnaArenden = openCases,
            OlastaNotiser = unreadNotifs,
            NastaPass = shifts.FirstOrDefault()?.Datum.ToString("ddd d/M") ?? "-",
            NastaPassTid = shifts.FirstOrDefault() != null
                ? $"{shifts.First().PlaneradStart:HH:mm}-{shifts.First().PlaneradSlut:HH:mm}"
                : "-"
        };
    }
}

public record DashboardData
{
    public string Fornamn { get; init; } = "";
    public string Efternamn { get; init; } = "";
    public int SemesterdagarKvar { get; init; }
    public int OppnaArenden { get; init; }
    public int OlastaNotiser { get; init; }
    public string NastaPass { get; init; } = "-";
    public string NastaPassTid { get; init; } = "-";
}
