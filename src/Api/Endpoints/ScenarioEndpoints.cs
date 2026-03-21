using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Analytics.Domain;

namespace RegionHR.Api.Endpoints;

public static class ScenarioEndpoints
{
    public static WebApplication MapScenarioEndpoints(this WebApplication app)
    {
        var scenarios = app.MapGroup("/api/v1/analytics/scenarier").WithTags("Analytics Scenarios").RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista scenarier
        // ============================================================

        scenarios.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var list = await db.PlanningScenarios
                .Include(s => s.Antaganden)
                .OrderByDescending(s => s.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(list.Select(s => new
            {
                s.Id, s.Namn, s.Beskrivning, BasÅr = s.BasÅr,
                s.Status, s.SkapadAv, s.SkapadVid,
                AntalAntaganden = s.Antaganden.Count
            }));
        }).WithName("ListScenarios");

        // ============================================================
        // Skapa scenario
        // ============================================================

        scenarios.MapPost("/", async (CreateScenarioRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var scenario = PlanningScenario.Skapa(req.Namn, req.Beskrivning, req.BasÅr, req.SkapadAv);

            foreach (var a in req.Antaganden ?? [])
            {
                var assumption = ScenarioAssumption.Skapa(scenario.Id, a.Typ, a.Värde, a.Beskrivning, a.EnhetId);
                scenario.Antaganden.Add(assumption);
            }

            db.PlanningScenarios.Add(scenario);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/analytics/scenarier/{scenario.Id}", new
            {
                scenario.Id, scenario.Namn, scenario.Beskrivning,
                BasÅr = scenario.BasÅr, scenario.Status, scenario.SkapadAv
            });
        }).WithName("CreateScenario");

        // ============================================================
        // Hämta scenario med resultat
        // ============================================================

        scenarios.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var scenario = await db.PlanningScenarios
                .Include(s => s.Antaganden)
                .Include(s => s.Resultat)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (scenario is null) return Results.NotFound();

            return Results.Ok(new
            {
                scenario.Id, scenario.Namn, scenario.Beskrivning,
                BasÅr = scenario.BasÅr, scenario.Status, scenario.SkapadAv, scenario.SkapadVid,
                Antaganden = scenario.Antaganden.Select(a => new
                {
                    a.Id, a.Typ, Värde = a.Värde, a.Beskrivning, a.EnhetId
                }),
                Resultat = scenario.Resultat.OrderBy(r => r.Period).Select(r => new
                {
                    r.Id, r.Period, r.HeadcountPrognos, FTEPrognos = r.FTEPrognos,
                    TotalLönekostnad = r.TotalLönekostnad, AGAvgifter = r.AGAvgifter,
                    TotalKostnad = r.TotalKostnad, DeltaMotBudget = r.DeltaMotBudget,
                    BeräknadVid = r.BeräknadVid
                })
            });
        }).WithName("GetScenario");

        // ============================================================
        // Beräkna scenario
        // ============================================================

        scenarios.MapPost("/{id:guid}/berakna", async (Guid id, ScenarioCalculationService calcService, CancellationToken ct) =>
        {
            var results = await calcService.BeräknaAsync(id, ct);

            return Results.Ok(new
            {
                ScenarioId = id,
                AntalPerioder = results.Count,
                Resultat = results.OrderBy(r => r.Period).Select(r => new
                {
                    r.Period, r.HeadcountPrognos, FTEPrognos = r.FTEPrognos,
                    TotalLönekostnad = r.TotalLönekostnad, AGAvgifter = r.AGAvgifter,
                    TotalKostnad = r.TotalKostnad, DeltaMotBudget = r.DeltaMotBudget
                })
            });
        }).WithName("CalculateScenario");

        return app;
    }
}

// Request DTOs
record CreateScenarioRequest(
    string Namn,
    string Beskrivning,
    int BasÅr,
    string SkapadAv,
    List<CreateAssumptionDto>? Antaganden);

record CreateAssumptionDto(
    string Typ,
    decimal Värde,
    string Beskrivning,
    Guid? EnhetId);
