using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Analytics.Domain;
using RegionHR.Reporting.Domain;

namespace RegionHR.Api.Endpoints;

public static class AnalyticsExpandedEndpoints
{
    public static WebApplication MapAnalyticsExpandedEndpoints(this WebApplication app)
    {
        var kpi = app.MapGroup("/api/v1/analytics/kpi").WithTags("Analytics KPI").RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista KPI:er med senaste snapshots
        // ============================================================

        kpi.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var definitions = await db.KPIDefinitions
                .Where(k => k.ArAktiv)
                .OrderBy(k => k.Kategori).ThenBy(k => k.Namn)
                .ToListAsync(ct);

            var defIds = definitions.Select(d => d.Id).ToList();

            // Get latest snapshot per KPI
            var latestSnapshots = await db.KPISnapshots
                .Where(s => defIds.Contains(s.KPIDefinitionId))
                .GroupBy(s => s.KPIDefinitionId)
                .Select(g => g.OrderByDescending(s => s.BeraknadVid).First())
                .ToListAsync(ct);

            var snapshotMap = latestSnapshots.ToDictionary(s => s.KPIDefinitionId);

            return Results.Ok(definitions.Select(d =>
            {
                snapshotMap.TryGetValue(d.Id, out var snapshot);
                return new
                {
                    d.Id,
                    d.Namn,
                    d.Kategori,
                    d.Enhet,
                    d.Riktning,
                    d.GronTroskel,
                    d.GulTroskel,
                    d.RodTroskel,
                    Varde = snapshot?.Varde,
                    Trend = snapshot?.Trend,
                    Period = snapshot?.Period,
                    JamforelseVarde = snapshot?.JamforelseVarde
                };
            }));
        }).WithName("ListKPIs");

        // ============================================================
        // KPI historik
        // ============================================================

        kpi.MapGet("/{id:guid}/historik", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var definition = await db.KPIDefinitions.FirstOrDefaultAsync(k => k.Id == id, ct);
            if (definition is null) return Results.NotFound();

            var snapshots = await db.KPISnapshots
                .Where(s => s.KPIDefinitionId == id)
                .OrderByDescending(s => s.BeraknadVid)
                .Take(20)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                definition.Id,
                definition.Namn,
                definition.Kategori,
                definition.Enhet,
                Historik = snapshots.Select(s => new
                {
                    s.Period,
                    s.Varde,
                    s.JamforelseVarde,
                    s.Trend,
                    s.BeraknadVid
                })
            });
        }).WithName("GetKPIHistory");

        // ============================================================
        // Beräkna KPI:er (trigga manuell beräkning)
        // ============================================================

        kpi.MapPost("/berakna", async (KPIBeraknaRequest? req, KPICalculationService calcService, CancellationToken ct) =>
        {
            var period = req?.Period ?? $"{DateTime.Today.Year}-Q{(DateTime.Today.Month - 1) / 3 + 1}";

            var snapshots = await calcService.CalculateAllAsync(period, ct);

            return Results.Ok(new
            {
                Period = period,
                AntalBeraknade = snapshots.Count,
                Resultat = snapshots.Select(s => new
                {
                    s.Id,
                    s.KPIDefinitionId,
                    s.Period,
                    s.Varde,
                    s.JamforelseVarde,
                    s.Trend,
                    s.BeraknadVid
                })
            });
        }).WithName("CalculateKPIs");

        // ============================================================
        // Prediktionsresultat
        // ============================================================

        var prediktion = app.MapGroup("/api/v1/analytics/prediktion").WithTags("Analytics Predictions").RequireAuthorization("ChefEllerHR");

        prediktion.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var models = await db.PredictionModels
                .OrderBy(m => m.Typ)
                .ToListAsync(ct);

            var modelIds = models.Select(m => m.Id).ToList();

            var latestResults = await db.PredictionResults
                .Where(r => modelIds.Contains(r.PredictionModelId))
                .OrderByDescending(r => r.BeraknadVid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                Modeller = models.Select(m => new
                {
                    m.Id,
                    m.Namn,
                    m.Typ,
                    m.Accuracy,
                    m.SenasteTranningsDatum,
                    AntalResultat = latestResults.Count(r => r.PredictionModelId == m.Id)
                }),
                SenasteResultat = latestResults.Select(r => new
                {
                    r.Id,
                    r.PredictionModelId,
                    r.EntityTyp,
                    r.EntityId,
                    r.Score,
                    r.RiskNiva,
                    r.BeraknadVid
                })
            });
        }).WithName("GetPredictions");

        // ============================================================
        // Skapa schemalagd rapport
        // ============================================================

        var schema = app.MapGroup("/api/v1/analytics/rapporter").WithTags("Analytics Reports").RequireAuthorization("ChefEllerHR");

        schema.MapPost("/schema", async (CreateScheduledReportRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            // Validate report template exists
            var template = await db.ReportDefinitions.FirstOrDefaultAsync(r => r.Id == req.ReportTemplateId, ct);
            if (template is null)
                return Results.BadRequest(new { error = "Rapportmallen finns inte" });

            var scheduled = ScheduledReport.Skapa(req.ReportTemplateId, req.Frekvens, req.Mottagare, req.Format);
            await db.ScheduledReports.AddAsync(scheduled, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/analytics/rapporter/schema/{scheduled.Id}", new
            {
                scheduled.Id,
                scheduled.ReportTemplateId,
                scheduled.Frekvens,
                scheduled.Mottagare,
                scheduled.Format,
                scheduled.NastaKorning
            });
        }).WithName("CreateScheduledReport");

        // ============================================================
        // Lista schemalagda rapporter
        // ============================================================

        schema.MapGet("/schema", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var scheduled = await db.ScheduledReports
                .OrderBy(s => s.NastaKorning)
                .ToListAsync(ct);

            return Results.Ok(scheduled.Select(s => new
            {
                s.Id,
                s.ReportTemplateId,
                s.Frekvens,
                s.Mottagare,
                s.Format,
                s.SenastKord,
                s.NastaKorning
            }));
        }).WithName("ListScheduledReports");

        return app;
    }
}

// Request DTOs
record KPIBeraknaRequest(string? Period);
record CreateScheduledReportRequest(Guid ReportTemplateId, string Frekvens, string Mottagare, string Format);
