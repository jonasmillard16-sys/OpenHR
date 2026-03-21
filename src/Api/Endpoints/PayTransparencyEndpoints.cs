using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Analytics.Domain;

namespace RegionHR.Api.Endpoints;

public static class PayTransparencyEndpoints
{
    public static WebApplication MapPayTransparencyEndpoints(this WebApplication app)
    {
        var transparency = app.MapGroup("/api/v1/analytics/lonetransparens")
            .WithTags("Pay Transparency")
            .RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista rapporter
        // ============================================================

        transparency.MapGet("/", async (int? ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.PayTransparencyReports.AsQueryable();
            if (ar.HasValue)
                query = query.Where(r => r.Ar == ar.Value);

            var reports = await query
                .OrderByDescending(r => r.Ar)
                .ThenByDescending(r => r.GenereradVid)
                .ToListAsync(ct);

            return Results.Ok(reports.Select(r => new
            {
                r.Id,
                r.Ar,
                r.RapportPeriod,
                r.Status,
                r.GenereradVid,
                r.PubliceradVid,
                r.TotalAnstallda,
                r.KonsGapProcent,
                r.MedianGapProcent
            }));
        }).WithName("ListPayTransparencyReports");

        // ============================================================
        // Beräkna ny rapport
        // ============================================================

        transparency.MapPost("/berakna", async (BeraknaPayTransparencyRequest? req,
            PayEquityCalculationService calcService, RegionHRDbContext db, CancellationToken ct) =>
        {
            var ar = req?.Ar ?? DateTime.Today.Year;

            var rapport = await calcService.BeraknaRapportAsync(ar, ct);

            db.PayTransparencyReports.Add(rapport);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/analytics/lonetransparens/{rapport.Id}", new
            {
                rapport.Id,
                rapport.Ar,
                rapport.RapportPeriod,
                rapport.Status,
                rapport.TotalAnstallda,
                rapport.KonsGapProcent,
                rapport.MedianGapProcent,
                AntalKategorier = rapport.Analyser.Count
            });
        }).WithName("CalculatePayTransparencyReport");

        // ============================================================
        // Rapport detalj
        // ============================================================

        transparency.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var rapport = await db.PayTransparencyReports
                .Include(r => r.Analyser)
                    .ThenInclude(a => a.Kohorter)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (rapport is null) return Results.NotFound();

            return Results.Ok(new
            {
                rapport.Id,
                rapport.Ar,
                rapport.RapportPeriod,
                rapport.Status,
                rapport.GenereradVid,
                rapport.PubliceradVid,
                rapport.TotalAnstallda,
                rapport.KonsGapProcent,
                rapport.MedianGapProcent,
                rapport.RapportData,
                Analyser = rapport.Analyser.Select(a => new
                {
                    a.Id,
                    a.Befattningskategori,
                    a.AntalKvinnor,
                    a.AntalMan,
                    a.MedelLonKvinnor,
                    a.MedelLonMan,
                    a.MedianLonKvinnor,
                    a.MedianLonMan,
                    a.OjusteratGapProcent,
                    a.JusteratGapProcent,
                    a.ForklarandeFaktorer,
                    a.Kraver5ProcentUtredning,
                    AtgardsKostnad = PayEquityCalculationService.BeraknaAtgardsKostnad(a),
                    Kohorter = a.Kohorter.Select(k => new
                    {
                        k.Id,
                        k.KohortNamn,
                        k.AntalAnstallda,
                        k.GapProcent,
                        k.TrendFranForraAret
                    })
                })
            });
        }).WithName("GetPayTransparencyReport");

        // ============================================================
        // Publicera rapport
        // ============================================================

        transparency.MapPost("/{id:guid}/publicera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var rapport = await db.PayTransparencyReports
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (rapport is null) return Results.NotFound();

            try
            {
                rapport.Publicera();
                await db.SaveChangesAsync(ct);

                return Results.Ok(new
                {
                    rapport.Id,
                    rapport.Status,
                    rapport.PubliceradVid,
                    Meddelande = "Rapporten har publicerats."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("PublishPayTransparencyReport");

        return app;
    }
}

record BeraknaPayTransparencyRequest(int? Ar);
