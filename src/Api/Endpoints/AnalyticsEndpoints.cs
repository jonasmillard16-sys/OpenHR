using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Analytics.Domain;

namespace RegionHR.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static WebApplication MapAnalyticsEndpoints(this WebApplication app)
    {
        var analytics = app.MapGroup("/api/v1/analytics").WithTags("Analytics").RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista sparade rapporter
        // ============================================================

        analytics.MapGet("/rapporter", async (Guid? skapadAv, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.SavedReports.AsQueryable();
            if (skapadAv.HasValue)
                query = query.Where(r => r.SkapadAvId == skapadAv.Value);

            var reports = await query.OrderByDescending(r => r.SkapadVid).ToListAsync(ct);

            return Results.Ok(reports.Select(r => new
            {
                r.Id, r.Namn, r.Beskrivning, r.SkapadAvId,
                r.Visualisering, r.ArDelad, r.SkapadVid, r.SenastKordVid
            }));
        }).WithName("ListSavedReports");

        // ============================================================
        // Skapa sparad rapport
        // ============================================================

        analytics.MapPost("/rapport", async (CreateSavedReportRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var report = SavedReport.Skapa(req.SkapadAvId, req.Namn, req.Beskrivning, req.QueryDefinition, req.Visualisering);
            await db.SavedReports.AddAsync(report, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/analytics/rapporter", new
            {
                report.Id, report.Namn, report.Beskrivning,
                report.SkapadAvId, report.Visualisering
            });
        }).WithName("CreateSavedReport");

        // ============================================================
        // Uppdatera sparad rapport
        // ============================================================

        analytics.MapPut("/rapport/{id:guid}", async (Guid id, UpdateSavedReportRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var report = await db.SavedReports.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (report is null) return Results.NotFound();

            report.Uppdatera(req.QueryDefinition, req.Visualisering);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                report.Id, report.Namn, report.Beskrivning,
                report.QueryDefinition, report.Visualisering
            });
        }).WithName("UpdateSavedReport");

        // ============================================================
        // Kör ad hoc-fråga (returnerar placeholder-resultat)
        // ============================================================

        analytics.MapPost("/rapport/{id:guid}/kor", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var report = await db.SavedReports.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (report is null) return Results.NotFound();

            report.MarkeraSomKord();
            await db.SaveChangesAsync(ct);

            // In production, this would parse QueryDefinition and execute a dynamic query
            return Results.Ok(new
            {
                report.Id, report.Namn,
                KordVid = report.SenastKordVid,
                Resultat = new { rader = 0, meddelande = "Ad hoc-fråga utförd (placeholder)" }
            });
        }).WithName("ExecuteSavedReport");

        // ============================================================
        // Lista dashboards
        // ============================================================

        analytics.MapGet("/dashboards", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var dashboards = await db.Dashboards
                .OrderBy(d => d.Namn)
                .ToListAsync(ct);

            return Results.Ok(dashboards.Select(d => new
            {
                d.Id, d.Namn, d.AgarId, d.ArDelad, d.SkapadVid
            }));
        }).WithName("ListDashboards");

        // ============================================================
        // Skapa dashboard
        // ============================================================

        analytics.MapPost("/dashboard", async (CreateDashboardRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var dashboard = Dashboard.Skapa(req.AgarId, req.Namn, req.Layout ?? "[]");
            await db.Dashboards.AddAsync(dashboard, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/analytics/dashboards", new
            {
                dashboard.Id, dashboard.Namn, dashboard.AgarId
            });
        }).WithName("CreateDashboard");

        // ============================================================
        // Uppdatera dashboard layout
        // ============================================================

        analytics.MapPut("/dashboard/{id:guid}", async (Guid id, UpdateDashboardRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var dashboard = await db.Dashboards.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (dashboard is null) return Results.NotFound();

            dashboard.UppdateraLayout(req.Layout);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                dashboard.Id, dashboard.Namn, dashboard.Layout
            });
        }).WithName("UpdateDashboardLayout");

        return app;
    }
}

// Request DTOs
record CreateSavedReportRequest(Guid SkapadAvId, string Namn, string Beskrivning, string QueryDefinition, string? Visualisering);
record UpdateSavedReportRequest(string QueryDefinition, string? Visualisering);
record CreateDashboardRequest(Guid AgarId, string Namn, string? Layout);
record UpdateDashboardRequest(string Layout);
