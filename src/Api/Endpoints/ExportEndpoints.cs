using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Export;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public enum ExportFormat
{
    Csv,
    Xlsx
}

public static class ExportEndpoints
{
    public static WebApplication MapExportEndpoints(this WebApplication app)
    {
        var export = app.MapGroup("/api/v1/export").WithTags("Export").RequireAuthorization("ChefEllerHR").RequireRateLimiting("export");

        // ============================================================
        // Exportera anställda
        // ============================================================

        export.MapGet("/anstallda", async (string format, ExportService exportService, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<ExportFormat>(format, true, out var exportFormat))
                return Results.BadRequest(new { error = $"Ogiltigt format: {format}. Giltiga värden: csv, xlsx" });

            var employees = await db.Employees.ToListAsync(ct);

            var headers = new[] { "Id", "Personnummer", "Förnamn", "Efternamn", "E-post", "Telefon" };

            if (exportFormat == ExportFormat.Csv)
            {
                var bytes = exportService.ToCsv(employees, headers, e => new[]
                {
                    e.Id.Value.ToString(),
                    e.Personnummer.ToString(),
                    e.Fornamn,
                    e.Efternamn,
                    e.Epost ?? "",
                    e.Telefon ?? ""
                });
                return Results.File(bytes, "text/csv", $"anstallda_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            else
            {
                var bytes = exportService.ToExcel(employees, "Anställda", headers, e => new object[]
                {
                    e.Id.Value.ToString(),
                    e.Personnummer.ToString(),
                    e.Fornamn,
                    e.Efternamn,
                    e.Epost ?? "",
                    e.Telefon ?? ""
                });
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"anstallda_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
        }).WithName("ExportEmployees");

        // ============================================================
        // Exportera löneresultat
        // ============================================================

        export.MapGet("/lonekorngar/{id:guid}", async (Guid id, string format, ExportService exportService, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<ExportFormat>(format, true, out var exportFormat))
                return Results.BadRequest(new { error = $"Ogiltigt format: {format}. Giltiga värden: csv, xlsx" });

            var results = await db.PayrollResults
                .Where(r => r.KorningsId == PayrollRunId.From(id))
                .ToListAsync(ct);

            if (!results.Any())
                return Results.NotFound();

            var headers = new[] { "AnställdId", "År", "Månad", "Brutto", "Skatt", "Netto", "Arbetsgivaravgifter" };

            if (exportFormat == ExportFormat.Csv)
            {
                var bytes = exportService.ToCsv(results, headers, r => new[]
                {
                    r.AnstallId.Value.ToString(),
                    r.Year.ToString(),
                    r.Month.ToString(),
                    r.Brutto.Amount.ToString("F2"),
                    r.Skatt.Amount.ToString("F2"),
                    r.Netto.Amount.ToString("F2"),
                    r.Arbetsgivaravgifter.Amount.ToString("F2")
                });
                return Results.File(bytes, "text/csv", $"loneresultat_{id:N}_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            else
            {
                var bytes = exportService.ToExcel(results, "Löneresultat", headers, r => new object[]
                {
                    r.AnstallId.Value.ToString(),
                    r.Year,
                    r.Month,
                    r.Brutto.Amount,
                    r.Skatt.Amount,
                    r.Netto.Amount,
                    r.Arbetsgivaravgifter.Amount
                });
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"loneresultat_{id:N}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
        }).WithName("ExportPayrollResults");

        return app;
    }
}
