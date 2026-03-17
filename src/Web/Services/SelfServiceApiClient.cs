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

    // For MittSchema.razor
    public async Task<List<SchemaPassData>> GetUpcomingShiftsAsync(EmployeeId id, int days = 7, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var end = today.AddDays(days);
        var shifts = await _db.ScheduledShifts
            .Where(s => s.AnstallId == id && s.Datum >= today && s.Datum <= end)
            .OrderBy(s => s.Datum)
            .ToListAsync(ct);

        return shifts.Select(s => new SchemaPassData
        {
            Datum = s.Datum,
            DagNamn = s.Datum.ToString("ddd d/M"),
            PassTyp = s.PassTyp.ToString(),
            Start = s.PlaneradStart.ToString("HH:mm"),
            Slut = s.PlaneradSlut.ToString("HH:mm"),
            Rast = s.Rast,
            Timmar = s.PlaneradeTimmar
        }).ToList();
    }

    // For MinLedighet.razor
    public async Task<SemesterSaldoData?> GetVacationBalanceAsync(Guid anstallId, int year, CancellationToken ct = default)
    {
        var balance = await _db.VacationBalances
            .FirstOrDefaultAsync(b => b.AnstallId == anstallId && b.Ar == year, ct);
        if (balance is null) return null;
        return new SemesterSaldoData
        {
            Tilldelning = balance.Tilldelning,
            UttagnaDagar = balance.UttagnaDagar,
            SparadeDagar = balance.SparadeDagar,
            TillgangligaDagar = balance.TillgangligaDagar
        };
    }

    // For MinLon.razor
    public async Task<List<LonespecData>> GetSalaryHistoryAsync(EmployeeId id, int limit = 12, CancellationToken ct = default)
    {
        var results = await _db.PayrollResults
            .Where(r => r.AnstallId == id)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Take(limit)
            .ToListAsync(ct);

        return results.Select(r => new LonespecData
        {
            Period = $"{r.Year}-{r.Month:D2}",
            Brutto = r.Brutto.Amount,
            Skatt = r.Skatt.Amount,
            Netto = r.Netto.Amount,
            OBTillagg = r.OBTillagg.Amount,
            Overtid = r.Overtidstillagg.Amount
        }).ToList();
    }

    // For MinaArenden.razor
    public async Task<List<ArendeData>> GetMyCasesAsync(EmployeeId id, CancellationToken ct = default)
    {
        var cases = await _db.Cases
            .Where(c => c.AnstallId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        return cases.Select(c => new ArendeData
        {
            Id = c.Id.Value,
            Typ = c.Typ.ToString(),
            Status = c.Status.ToString(),
            Beskrivning = c.Beskrivning ?? "",
            Datum = c.CreatedAt.ToString("yyyy-MM-dd")
        }).ToList();
    }

    // For MinProfil.razor
    public async Task<ProfilData?> GetProfileAsync(EmployeeId id, CancellationToken ct = default)
    {
        var emp = await _db.Employees.Include(e => e.Anstallningar)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return null;

        var aktiv = emp.AktivAnstallning(DateOnly.FromDateTime(DateTime.Today));
        return new ProfilData
        {
            Fornamn = emp.Fornamn,
            Efternamn = emp.Efternamn,
            PersonnummerMaskerat = emp.Personnummer.ToMaskedString(),
            Epost = emp.Epost ?? "",
            Telefon = emp.Telefon ?? "",
            Adress = emp.Adress?.Gatuadress ?? "",
            Postnummer = emp.Adress?.Postnummer ?? "",
            Ort = emp.Adress?.Ort ?? "",
            Befattning = aktiv?.Befattningstitel ?? "-",
            Enhet = "-", // TODO: resolve org unit name
            AnstaldSedan = aktiv?.Giltighetsperiod.Start.ToString("yyyy-MM-dd") ?? "-"
        };
    }

    public async Task SaveContactInfoAsync(EmployeeId id, string? epost, string? telefon, string? gatuadress, string? postnummer, string? ort, CancellationToken ct = default)
    {
        var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return;

        Address? adress = gatuadress != null ? new Address(gatuadress, postnummer ?? "", ort ?? "") : null;
        emp.UppdateraKontaktuppgifter(epost ?? emp.Epost, telefon ?? emp.Telefon, adress ?? emp.Adress);
        await _db.SaveChangesAsync(ct);
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

public record SchemaPassData
{
    public DateOnly Datum { get; init; }
    public string DagNamn { get; init; } = "";
    public string PassTyp { get; init; } = "";
    public string Start { get; init; } = "";
    public string Slut { get; init; } = "";
    public TimeSpan Rast { get; init; }
    public decimal Timmar { get; init; }
}

public record SemesterSaldoData
{
    public int Tilldelning { get; init; }
    public int UttagnaDagar { get; init; }
    public int SparadeDagar { get; init; }
    public int TillgangligaDagar { get; init; }
}

public record LonespecData
{
    public string Period { get; init; } = "";
    public decimal Brutto { get; init; }
    public decimal Skatt { get; init; }
    public decimal Netto { get; init; }
    public decimal OBTillagg { get; init; }
    public decimal Overtid { get; init; }
}

public record ArendeData
{
    public Guid Id { get; init; }
    public string Typ { get; init; } = "";
    public string Status { get; init; } = "";
    public string Beskrivning { get; init; } = "";
    public string Datum { get; init; } = "";
}

public record ProfilData
{
    public string Fornamn { get; init; } = "";
    public string Efternamn { get; init; } = "";
    public string PersonnummerMaskerat { get; init; } = "";
    public string Epost { get; init; } = "";
    public string Telefon { get; init; } = "";
    public string Adress { get; init; } = "";
    public string Postnummer { get; init; } = "";
    public string Ort { get; init; } = "";
    public string Befattning { get; init; } = "";
    public string Enhet { get; init; } = "";
    public string AnstaldSedan { get; init; } = "";
}
