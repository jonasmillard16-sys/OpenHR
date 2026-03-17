using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Export;
using RegionHR.Reporting.Domain;

namespace RegionHR.Infrastructure.Reporting;

public class ReportGenerator
{
    private readonly RegionHRDbContext _db;
    private readonly ExportService _export;

    public ReportGenerator(RegionHRDbContext db, ExportService export)
    {
        _db = db;
        _export = export;
    }

    public async Task<byte[]> GenerateAsync(ReportType type, CancellationToken ct = default)
    {
        return type switch
        {
            ReportType.Personalrostter => await GeneratePersonalrostar(ct),
            ReportType.Loneregister => await GenerateLoneregister(ct),
            ReportType.Franvarostatistik => await GenerateFranvarostatistik(ct),
            ReportType.Overtidsrapport => await GenerateOvertidsrapport(ct),
            ReportType.LASStatus => await GenerateLASStatus(ct),
            ReportType.SjukfranvaroKPI => await GenerateSjukfranvaroKPI(ct),
            ReportType.KostnadPerEnhet => await GenerateKostnadPerEnhet(ct),
            _ => await GeneratePersonalrostar(ct)
        };
    }

    private async Task<byte[]> GeneratePersonalrostar(CancellationToken ct)
    {
        var employees = await _db.Employees.Include(e => e.Anstallningar).OrderBy(e => e.Efternamn).ToListAsync(ct);
        var headers = new[] { "Fornamn", "Efternamn", "Personnummer", "E-post", "Anstallningsform", "Sysselsattningsgrad" };
        return _export.ToExcel(employees, "Personalrostar", headers, e => new object[]
        {
            e.Fornamn, e.Efternamn, e.Personnummer.ToMaskedString(), e.Epost ?? "",
            e.Anstallningar.FirstOrDefault()?.Anstallningsform.ToString() ?? "",
            e.Anstallningar.FirstOrDefault()?.Sysselsattningsgrad ?? (object)0m
        });
    }

    private async Task<byte[]> GenerateLoneregister(CancellationToken ct)
    {
        var results = await _db.PayrollResults
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .ToListAsync(ct);

        var headers = new[] { "AnstallId", "Ar", "Manad", "Brutto", "Skatt", "Netto", "Arbetsgivaravgifter", "OB-tillagg", "Overtid" };
        return _export.ToExcel(results, "Loneregister", headers, r => new object[]
        {
            r.AnstallId.ToString(), r.Year, r.Month,
            r.Brutto.Amount, r.Skatt.Amount, r.Netto.Amount,
            r.Arbetsgivaravgifter.Amount, r.OBTillagg.Amount, r.Overtidstillagg.Amount
        });
    }

    private async Task<byte[]> GenerateFranvarostatistik(CancellationToken ct)
    {
        var leaveRequests = await _db.LeaveRequests
            .Where(r => r.Status == Leave.Domain.LeaveRequestStatus.Godkand)
            .ToListAsync(ct);

        var grouped = leaveRequests
            .GroupBy(r => r.Typ)
            .Select(g => new { Typ = g.Key.ToString(), AntalBegaran = g.Count(), TotalaDagar = g.Sum(r => r.AntalDagar) })
            .OrderByDescending(g => g.TotalaDagar)
            .ToList();

        var headers = new[] { "Franvarotyp", "Antal begaran", "Totala dagar" };
        return _export.ToExcel(grouped, "Franvarostatistik", headers, g => new object[]
        {
            g.Typ, g.AntalBegaran, g.TotalaDagar
        });
    }

    private async Task<byte[]> GenerateOvertidsrapport(CancellationToken ct)
    {
        var timesheets = await _db.Timesheets
            .Where(t => t.Overtid > 0)
            .OrderByDescending(t => t.Ar).ThenByDescending(t => t.Manad)
            .ToListAsync(ct);

        var headers = new[] { "AnstallId", "Ar", "Manad", "Planerade timmar", "Faktiska timmar", "Overtid", "Status" };
        return _export.ToExcel(timesheets, "Overtidsrapport", headers, t => new object[]
        {
            t.AnstallId.ToString(), t.Ar, t.Manad,
            t.PlaneradeTimmar, t.FaktiskaTimmar, t.Overtid, t.Status.ToString()
        });
    }

    private async Task<byte[]> GenerateLASStatus(CancellationToken ct)
    {
        var accumulations = await _db.LASAccumulations
            .OrderByDescending(a => a.AckumuleradeDagar)
            .ToListAsync(ct);

        var headers = new[] { "AnstallId", "Anstallningsform", "Ackumulerade dagar", "Status", "Konverteringsdatum", "Foretradesratt" };
        return _export.ToExcel(accumulations, "LAS-Status", headers, a => new object[]
        {
            a.AnstallId.ToString(), a.Anstallningsform.ToString(),
            a.AckumuleradeDagar, a.Status.ToString(),
            a.KonverteringsDatum?.ToString("yyyy-MM-dd") ?? "",
            a.HarForetradesratt ? "Ja" : "Nej"
        });
    }

    private async Task<byte[]> GenerateSjukfranvaroKPI(CancellationToken ct)
    {
        var sickLeaves = await _db.LeaveRequests
            .Where(r => r.Typ == Leave.Domain.LeaveType.Sjukfranvaro && r.Status == Leave.Domain.LeaveRequestStatus.Godkand)
            .ToListAsync(ct);

        var grouped = sickLeaves
            .GroupBy(r => new { r.FranDatum.Year, r.FranDatum.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                AntalTillfallen = g.Count(),
                TotalaDagar = g.Sum(r => r.AntalDagar),
                AntalAnstallda = g.Select(r => r.AnstallId).Distinct().Count()
            })
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
            .ToList();

        var headers = new[] { "Ar", "Manad", "Antal tillfallen", "Totala dagar", "Antal anstallda" };
        return _export.ToExcel(grouped, "Sjukfranvaro-KPI", headers, g => new object[]
        {
            g.Year, g.Month, g.AntalTillfallen, g.TotalaDagar, g.AntalAnstallda
        });
    }

    private async Task<byte[]> GenerateKostnadPerEnhet(CancellationToken ct)
    {
        var payrollResults = await _db.PayrollResults.ToListAsync(ct);
        var employments = await _db.Employments.ToListAsync(ct);

        var joined = from pr in payrollResults
                     join emp in employments on pr.AnstallningsId equals emp.Id into empJoin
                     from emp in empJoin.DefaultIfEmpty()
                     group new { pr, emp } by emp?.EnhetId.ToString() ?? "Okand" into g
                     select new
                     {
                         Enhet = g.Key,
                         TotalBrutto = g.Sum(x => x.pr.Brutto.Amount),
                         TotalArbetsgivaravgifter = g.Sum(x => x.pr.Arbetsgivaravgifter.Amount),
                         TotalKostnad = g.Sum(x => x.pr.Brutto.Amount + x.pr.Arbetsgivaravgifter.Amount),
                         AntalAnstallda = g.Select(x => x.pr.AnstallId).Distinct().Count()
                     };

        var data = joined.OrderByDescending(x => x.TotalKostnad).ToList();

        var headers = new[] { "Enhet", "Total brutto", "Arbetsgivaravgifter", "Total kostnad", "Antal anstallda" };
        return _export.ToExcel(data, "Kostnad per enhet", headers, d => new object[]
        {
            d.Enhet, d.TotalBrutto, d.TotalArbetsgivaravgifter, d.TotalKostnad, d.AntalAnstallda
        });
    }
}
