using System.Text.Json;
using RegionHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.GDPR;

public class RegisterutdragGenerator
{
    private readonly RegionHRDbContext _db;

    public RegisterutdragGenerator(RegionHRDbContext db) => _db = db;

    public async Task<string> GenerateAsync(Guid anstallId, CancellationToken ct = default)
    {
        var empId = EmployeeId.From(anstallId);

        var employee = await _db.Employees.Include(e => e.Anstallningar)
            .FirstOrDefaultAsync(e => e.Id == empId, ct);

        var cases = await _db.Cases
            .Where(c => c.AnstallId == empId)
            .ToListAsync(ct);

        var payrollResults = await _db.PayrollResults
            .Where(r => r.AnstallId == empId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Take(24)
            .ToListAsync(ct);

        var leaveRequests = await _db.LeaveRequests
            .Where(r => r.AnstallId == anstallId)
            .ToListAsync(ct);

        var documents = await _db.Documents
            .Where(d => d.AnstallId == anstallId && !d.IsArchived)
            .ToListAsync(ct);

        var certifications = await _db.Certifications
            .Where(c => c.AnstallId == anstallId)
            .ToListAsync(ct);

        var performanceReviews = await _db.PerformanceReviews
            .Where(r => r.AnstallId == anstallId)
            .ToListAsync(ct);

        var lasAccumulations = await _db.LASAccumulations
            .Where(a => a.AnstallId == empId)
            .ToListAsync(ct);

        var rehabCases = await _db.RehabCases
            .Where(r => r.AnstallId == empId)
            .ToListAsync(ct);

        var auditEntries = await _db.AuditEntries
            .Where(a => a.EntityId == anstallId.ToString())
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync(ct);

        var registerutdrag = new
        {
            genererat = DateTime.UtcNow,
            beskrivning = "Registerutdrag enligt GDPR Art 15 - Ratt till tillgang",
            personuppgifter = employee != null ? new
            {
                fornamn = employee.Fornamn,
                efternamn = employee.Efternamn,
                personnummer = employee.Personnummer.ToMaskedString(),
                epost = employee.Epost,
                telefon = employee.Telefon,
                anstallningar = employee.Anstallningar.Select(a => new
                {
                    Anstallningsform = a.Anstallningsform.ToString(),
                    StartDatum = a.Giltighetsperiod.Start,
                    SlutDatum = a.Giltighetsperiod.End,
                    Sysselsattningsgrad = a.Sysselsattningsgrad
                })
            } : null,
            arenden = cases.Select(c => new { Id = c.Id.Value, Typ = c.Typ.ToString(), Status = c.Status.ToString(), c.CreatedAt }),
            loneresultat = payrollResults.Select(r => new { r.Year, r.Month, Brutto = r.Brutto.Amount, Netto = r.Netto.Amount }),
            ledighetsansokningar = leaveRequests.Select(r => new { r.Id, Typ = r.Typ.ToString(), r.FranDatum, r.TillDatum, Status = r.Status.ToString() }),
            dokument = documents.Select(d => new { d.Id, d.FileName, Kategori = d.Kategori.ToString(), d.UppladdadVid }),
            certifieringar = certifications.Select(c => new { c.Namn, Typ = c.Typ.ToString(), c.GiltigFran, c.GiltigTill }),
            medarbetarsamtal = performanceReviews.Select(r => new { r.Ar, Status = r.Status.ToString(), r.OverallRating }),
            lasUppfoljning = lasAccumulations.Select(a => new { Anstallningsform = a.Anstallningsform.ToString(), a.AckumuleradeDagar, Status = a.Status.ToString() }),
            rehabilitering = rehabCases.Select(r => new { r.Id, Trigger = r.Trigger.ToString(), Status = r.Status.ToString() }),
            granskningslogg = auditEntries.Select(a => new { a.EntityType, Action = a.Action.ToString(), a.Timestamp })
        };

        return JsonSerializer.Serialize(registerutdrag, new JsonSerializerOptions { WriteIndented = true });
    }
}
