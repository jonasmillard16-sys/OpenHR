using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Analytics.Domain;

namespace RegionHR.Api.Endpoints;

public static class ONAEndpoints
{
    public static WebApplication MapONAEndpoints(this WebApplication app)
    {
        var ona = app.MapGroup("/api/v1/ona").WithTags("ONA").RequireAuthorization("Systemadmin");

        // ============================================================
        // Lista ONA-undersökningar
        // ============================================================

        ona.MapGet("/surveys", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var surveys = await db.ONASurveys
                .OrderByDescending(s => s.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(surveys.Select(s => new
            {
                s.Id, s.Namn, s.Period,
                Status = s.Status.ToString(),
                s.Fragor, s.SkapadVid
            }));
        }).WithName("ListONASurveys");

        // ============================================================
        // Skapa ONA-undersökning
        // ============================================================

        ona.MapPost("/survey", async (CreateONASurveyRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var survey = ONASurvey.Skapa(req.Namn, req.Period, req.Fragor ?? "[]");
            await db.ONASurveys.AddAsync(survey, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/ona/surveys", new
            {
                survey.Id, survey.Namn, survey.Period,
                Status = survey.Status.ToString()
            });
        }).WithName("CreateONASurvey");

        // ============================================================
        // Öppna undersökning
        // ============================================================

        ona.MapPost("/survey/{id:guid}/oppna", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var survey = await db.ONASurveys.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (survey is null) return Results.NotFound();

            try
            {
                survey.Oppna();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { survey.Id, Status = survey.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("OpenONASurvey");

        // ============================================================
        // Stäng och analysera undersökning
        // ============================================================

        ona.MapPost("/survey/{id:guid}/analysera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var survey = await db.ONASurveys.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (survey is null) return Results.NotFound();

            try
            {
                if (survey.Status == ONASurveyStatus.Open)
                    survey.Stang();

                // Fetch responses
                var responses = await db.ONAResponses
                    .Where(r => r.SurveyId == id)
                    .ToListAsync(ct);

                // Calculate network
                var result = ONACalculationService.Berakna(id, responses);

                // Remove existing analysis for this survey
                var existingNodes = await db.NetworkNodes.Where(n => n.SurveyId == id).ToListAsync(ct);
                var existingEdges = await db.NetworkEdges.Where(e => e.SurveyId == id).ToListAsync(ct);
                db.NetworkNodes.RemoveRange(existingNodes);
                db.NetworkEdges.RemoveRange(existingEdges);

                // Save new analysis
                await db.NetworkNodes.AddRangeAsync(result.Nodes, ct);
                await db.NetworkEdges.AddRangeAsync(result.Edges, ct);

                survey.Analysera();
                await db.SaveChangesAsync(ct);

                return Results.Ok(new
                {
                    survey.Id,
                    Status = survey.Status.ToString(),
                    AntalNoder = result.Nodes.Count,
                    AntalKanter = result.Edges.Count
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("AnalyzeONASurvey");

        // ============================================================
        // Hämta nätverksresultat
        // ============================================================

        ona.MapGet("/survey/{id:guid}/resultat", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var survey = await db.ONASurveys.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (survey is null) return Results.NotFound();

            var nodes = await db.NetworkNodes
                .Where(n => n.SurveyId == id)
                .OrderByDescending(n => n.BetweennessCentrality)
                .ToListAsync(ct);

            var edges = await db.NetworkEdges
                .Where(e => e.SurveyId == id)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                survey = new { survey.Id, survey.Namn, survey.Period, Status = survey.Status.ToString() },
                noder = nodes.Select(n => new
                {
                    n.Id, n.AnstallId, n.InDegree, n.OutDegree,
                    n.BetweennessCentrality, n.Kluster, n.Roll
                }),
                kanter = edges.Select(e => new
                {
                    e.Id, e.FranAnstallId, e.TillAnstallId,
                    e.FrageIndex, e.Styrka
                })
            });
        }).WithName("GetONAResults");

        return app;
    }
}

// Request DTOs
record CreateONASurveyRequest(string Namn, string Period, string? Fragor);
