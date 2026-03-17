using RegionHR.Core.Contracts;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace RegionHR.Web.Services;

public class AnstallningService
{
    private readonly RegionHRDbContext _db;

    public AnstallningService(RegionHRDbContext db) => _db = db;

    public async Task<List<EmployeeListItem>> HamtaAllaAsync(string? sokterm = null, CancellationToken ct = default)
    {
        var query = _db.Employees
            .Include(e => e.Anstallningar)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sokterm))
        {
            var term = sokterm.ToLower();
            query = query.Where(e =>
                e.Fornamn.ToLower().Contains(term) ||
                e.Efternamn.ToLower().Contains(term));
        }

        var employees = await query.OrderBy(e => e.Efternamn).Take(100).ToListAsync(ct);

        return employees.Select(e =>
        {
            var aktiv = e.Anstallningar.FirstOrDefault(a =>
                a.Giltighetsperiod.Start <= DateOnly.FromDateTime(DateTime.Today) &&
                (a.Giltighetsperiod.End == null || a.Giltighetsperiod.End >= DateOnly.FromDateTime(DateTime.Today)));
            return new EmployeeListItem(
                e.Id,
                e.Fornamn,
                e.Efternamn,
                e.Personnummer.ToMaskedString(),
                e.Epost,
                aktiv?.Befattningstitel ?? "-",
                aktiv?.Anstallningsform.ToString() ?? "-",
                aktiv?.Sysselsattningsgrad.Value ?? 0);
        }).ToList();
    }

    public async Task<Employee?> HamtaAsync(EmployeeId id, CancellationToken ct = default)
    {
        return await _db.Employees
            .Include(e => e.Anstallningar)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<EmployeeId> SkapaAsync(
        string personnummer, string fornamn, string efternamn,
        string? epost = null, CancellationToken ct = default)
    {
        var pnr = new Personnummer(personnummer);
        var employee = Employee.Skapa(pnr, fornamn, efternamn);
        if (epost is not null)
            employee.UppdateraKontaktuppgifter(epost, null, null);

        await _db.Employees.AddAsync(employee, ct);
        await _db.SaveChangesAsync(ct);
        return employee.Id;
    }

    public async Task UppdateraKontaktuppgifterAsync(
        EmployeeId id, string? epost, string? telefon, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null) return;
        employee.UppdateraKontaktuppgifter(epost, telefon, null);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UppdateraKontaktuppgifterMedAdressAsync(
        EmployeeId id, string? epost, string? telefon, Address? adress, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null) return;
        employee.UppdateraKontaktuppgifter(epost, telefon, adress);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UppdateraSkatteuppgifterAsync(
        EmployeeId id, int skattetabell, int skattekolumn, string kommun,
        decimal kommunalSkattesats, bool harKyrkoavgift, decimal? kyrkoavgiftssats,
        CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null) return;
        employee.UppdateraSkatteuppgifter(skattetabell, skattekolumn, kommun, kommunalSkattesats, harKyrkoavgift, kyrkoavgiftssats);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<OrganizationUnit>> HamtaOrganisationAsync(CancellationToken ct = default)
    {
        return await _db.OrganizationUnits
            .Include(o => o.Underenheter)
            .Where(o => o.OverordnadEnhetId == null)
            .ToListAsync(ct);
    }
}

public record EmployeeListItem(
    EmployeeId Id,
    string Fornamn,
    string Efternamn,
    string PersonnummerMaskerat,
    string? Epost,
    string Befattning,
    string Anstallningsform,
    decimal Sysselsattningsgrad);
