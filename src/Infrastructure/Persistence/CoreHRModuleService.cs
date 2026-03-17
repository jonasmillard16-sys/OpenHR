using Microsoft.EntityFrameworkCore;
using RegionHR.Core.Contracts;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence;

/// <summary>
/// Implementerar det publika kontraktet för Core HR-modulen.
/// Andra moduler använder detta interface för att läsa personaldata.
/// </summary>
public class CoreHRModuleService : ICoreHRModule
{
    private readonly RegionHRDbContext _db;

    public CoreHRModuleService(RegionHRDbContext db) => _db = db;

    public async Task<EmployeeDto?> GetEmployeeAsync(EmployeeId id, CancellationToken ct = default)
    {
        var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (emp is null) return null;

        return new EmployeeDto(
            emp.Id, emp.Fornamn, emp.Efternamn, emp.Personnummer.ToMaskedString(),
            emp.Epost, emp.Skattetabell, emp.Skattekolumn, emp.Kommun,
            emp.KommunalSkattesats, emp.HarKyrkoavgift, emp.Kyrkoavgiftssats,
            emp.HarJamkning, emp.JamkningBelopp?.Amount);
    }

    public async Task<EmploymentDto?> GetActiveEmploymentAsync(EmployeeId id, DateOnly date, CancellationToken ct = default)
    {
        var emp = await _db.Employments
            .FirstOrDefaultAsync(e => e.AnstallId == id &&
                e.Giltighetsperiod.Start <= date &&
                (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= date), ct);
        if (emp is null) return null;
        return MapEmployment(emp);
    }

    public async Task<IReadOnlyList<EmploymentDto>> GetActiveEmploymentsAsync(EmployeeId id, DateOnly date, CancellationToken ct = default)
    {
        var emps = await _db.Employments
            .Where(e => e.AnstallId == id &&
                e.Giltighetsperiod.Start <= date &&
                (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= date))
            .ToListAsync(ct);
        return emps.Select(MapEmployment).ToList();
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetEmployeesByUnitAsync(OrganizationId unitId, DateOnly date, CancellationToken ct = default)
    {
        var employeeIds = await _db.Employments
            .Where(e => e.EnhetId == unitId &&
                e.Giltighetsperiod.Start <= date &&
                (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= date))
            .Select(e => e.AnstallId)
            .Distinct()
            .ToListAsync(ct);

        var employees = await _db.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync(ct);

        return employees.Select(e => new EmployeeDto(
            e.Id, e.Fornamn, e.Efternamn, e.Personnummer.ToMaskedString(),
            e.Epost, e.Skattetabell, e.Skattekolumn, e.Kommun,
            e.KommunalSkattesats, e.HarKyrkoavgift, e.Kyrkoavgiftssats,
            e.HarJamkning, e.JamkningBelopp?.Amount)).ToList();
    }

    public async Task<OrganizationUnitDto?> GetOrganizationUnitAsync(OrganizationId id, CancellationToken ct = default)
    {
        var unit = await _db.OrganizationUnits.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (unit is null) return null;
        return new OrganizationUnitDto(unit.Id, unit.Namn, unit.Typ, unit.Kostnadsstalle, unit.OverordnadEnhetId);
    }

    private static EmploymentDto MapEmployment(RegionHR.Core.Domain.Employment emp) => new(
        emp.Id, emp.AnstallId, emp.EnhetId, emp.Anstallningsform, emp.Kollektivavtal,
        emp.Manadslon.Amount, emp.Sysselsattningsgrad.Value,
        emp.Giltighetsperiod.Start, emp.Giltighetsperiod.End, emp.BESTAKod);
}
