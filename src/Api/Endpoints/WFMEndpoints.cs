using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Scheduling.Domain;
using RegionHR.Scheduling.Optimization;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class WFMEndpoints
{
    public static WebApplication MapWFMEndpoints(this WebApplication app)
    {
        var wfm = app.MapGroup("/api/v1/wfm").WithTags("WFM").RequireAuthorization();

        // ============================================================
        // Prognos — Demand Forecast
        // ============================================================

        wfm.MapGet("/prognos/{enhetId:guid}", async (
            Guid enhetId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var forecasts = await db.DemandForecasts
                .Where(f => f.EnhetId == OrganizationId.From(enhetId))
                .OrderBy(f => f.Datum)
                .Take(100)
                .ToListAsync(ct);

            var patterns = await db.DemandPatterns
                .Where(p => p.EnhetId == OrganizationId.From(enhetId))
                .ToListAsync(ct);

            var events = await db.DemandEvents
                .OrderBy(e => e.DatumFran)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                EnhetId = enhetId,
                Prognoser = forecasts.Select(f => new
                {
                    f.Id, f.Datum, f.BeraknatAntal, f.BeraknadeTidmmar,
                    f.Konfidensgrad, f.BeraknadVid
                }),
                Monster = patterns.Select(p => new
                {
                    p.Id, p.Veckodag, p.TimPaAret,
                    p.GenomsnittligBelastning, p.SasongsVariation
                }),
                Handelser = events.Select(e => new
                {
                    e.Id, e.Namn, e.Typ, e.PaverkanGrad,
                    e.DatumFran, e.DatumTill
                })
            });
        }).WithName("GetDemandForecast");

        // ============================================================
        // Optimering — Start Scheduling Run
        // ============================================================

        wfm.MapPost("/optimering", async (
            StartOptimeringRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var run = SchedulingRun.Starta(
                OrganizationId.From(req.EnhetId),
                req.PeriodFran,
                req.PeriodTill,
                System.Text.Json.JsonSerializer.Serialize(new { req.EnhetId, req.PeriodFran, req.PeriodTill }));

            // Run optimization using ConstraintScheduleSolver
            var solver = new ConstraintScheduleSolver();

            // Fetch available staff for the unit
            var unitEmployees = await db.Employees
                .Include(e => e.Anstallningar)
                .ToListAsync(ct);

            var datum = DateOnly.FromDateTime(DateTime.Today);
            var personal = unitEmployees
                .Where(e => e.Anstallningar.Any(a =>
                    a.EnhetId == OrganizationId.From(req.EnhetId) &&
                    a.Giltighetsperiod.IsActiveOn(datum)))
                .Select(e => new PersonalInfo
                {
                    AnstallId = e.Id,
                    Namn = $"{e.Fornamn} {e.Efternamn}",
                    Sysselsattningsgrad = 100m,
                    Kompetenser = [],
                    LedigaDagar = []
                })
                .ToList();

            // Generate requirements for the period
            var behov = new List<StaffingRequirement>();
            for (var d = req.PeriodFran; d <= req.PeriodTill; d = d.AddDays(1))
            {
                behov.Add(new StaffingRequirement
                {
                    Datum = d, PassTyp = ShiftType.Dag,
                    Start = new TimeOnly(7, 0), Slut = new TimeOnly(16, 0),
                    Rast = TimeSpan.FromMinutes(60), AntalBehov = 2
                });
                behov.Add(new StaffingRequirement
                {
                    Datum = d, PassTyp = ShiftType.Kvall,
                    Start = new TimeOnly(15, 0), Slut = new TimeOnly(22, 0),
                    Rast = TimeSpan.FromMinutes(30), AntalBehov = 1
                });
            }

            var problem = new ScheduleProblem
            {
                EnhetId = OrganizationId.From(req.EnhetId),
                Period = new DateRange(req.PeriodFran, req.PeriodTill),
                PassBehov = behov,
                TillgangligPersonal = personal
            };

            try
            {
                var solution = solver.Solve(problem);
                run.Slutfor(
                    solution.Tilldelningar.Count,
                    solution.TotalKostnad.Amount,
                    0m,
                    solution.ObemannadeBehov.Count == 0);
            }
            catch
            {
                run.MarkFailed();
            }

            await db.SchedulingRuns.AddAsync(run, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/wfm/optimering/{run.Id}", new
            {
                run.Id, run.Status, run.GenereradePass,
                run.TotalOBKostnad, run.TotalOvertidKostnad,
                run.ATLKompliant, run.SkapadVid
            });
        }).WithName("StartOptimization");

        wfm.MapGet("/optimering", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var runs = await db.SchedulingRuns
                .OrderByDescending(r => r.SkapadVid)
                .Take(20)
                .ToListAsync(ct);

            return Results.Ok(runs.Select(r => new
            {
                r.Id, r.EnhetId, r.PeriodFran, r.PeriodTill,
                r.GenereradePass, r.TotalOBKostnad, r.TotalOvertidKostnad,
                r.ATLKompliant, r.Status, r.SkapadVid
            }));
        }).WithName("ListOptimizationRuns");

        // ============================================================
        // Trötthet — Fatigue Score
        // ============================================================

        wfm.MapGet("/trotthet/{anstallId:guid}", async (
            Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var score = await db.FatigueScores
                .Where(f => f.AnstallId == EmployeeId.From(anstallId))
                .OrderByDescending(f => f.BeraknadVid)
                .FirstOrDefaultAsync(ct);

            if (score is null)
                return Results.NotFound(new { message = "Ingen trötthetspoäng hittad." });

            return Results.Ok(new
            {
                score.Id, score.AnstallId, score.Poang,
                score.KonsekutivaDagar, score.NattpassSenaste7Dagar,
                score.TotalTimmarSenaste7Dagar, score.KortVila,
                score.HelgarbeteSenaste4Veckor, score.BeraknadVid,
                Riskniva = score.Poang switch
                {
                    >= 70 => "Hog",
                    >= 40 => "Medel",
                    _ => "Lag"
                }
            });
        }).WithName("GetFatigueScore");

        // ============================================================
        // Täckning — Shift Coverage Request
        // ============================================================

        wfm.MapPost("/tackning", async (
            CreateCoverageRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var coverage = ShiftCoverageRequest.Skapa(req.ScheduledShiftId, req.Anledning);

            await db.ShiftCoverageRequests.AddAsync(coverage, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/wfm/tackning/{coverage.Id}", new
            {
                coverage.Id, coverage.ScheduledShiftId, coverage.Anledning,
                Status = coverage.Status.ToString(), coverage.SkapadVid
            });
        }).WithName("CreateCoverageRequest");

        wfm.MapGet("/tackning", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var requests = await db.ShiftCoverageRequests
                .OrderByDescending(r => r.SkapadVid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(requests.Select(r => new
            {
                r.Id, r.ScheduledShiftId, r.Anledning,
                Status = r.Status.ToString(),
                r.TilldeladAnstallId, r.SkapadVid
            }));
        }).WithName("ListCoverageRequests");

        wfm.MapPost("/tackning/{id:guid}/tilldela", async (
            Guid id, TilldelaCoverageRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var coverage = await db.ShiftCoverageRequests.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (coverage is null) return Results.NotFound();

            try
            {
                coverage.Tilldela(EmployeeId.From(req.AnstallId));
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { coverage.Id, Status = coverage.Status.ToString(), coverage.TilldeladAnstallId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("AssignCoverage");

        // ============================================================
        // Tillgänglighet — Employee Availability
        // ============================================================

        wfm.MapGet("/tillganglighet/{anstallId:guid}", async (
            Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var avail = await db.EmployeeAvailabilities
                .Where(a => a.AnstallId == EmployeeId.From(anstallId))
                .ToListAsync(ct);

            return Results.Ok(avail.Select(a => new
            {
                a.Id, a.AnstallId, a.Veckodag, a.Datum,
                a.TidFran, a.TidTill, a.Preferens, a.ArRepeterande
            }));
        }).WithName("GetEmployeeAvailability");

        // ============================================================
        // Constraints
        // ============================================================

        wfm.MapGet("/constraints", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var constraints = await db.SchedulingConstraints.ToListAsync(ct);
            return Results.Ok(constraints.Select(c => new
            {
                c.Id, c.Typ, c.Beskrivning, c.Vikt, c.ArHard
            }));
        }).WithName("ListSchedulingConstraints");

        return app;
    }
}

// Request DTOs for WFM
record StartOptimeringRequest(Guid EnhetId, DateOnly PeriodFran, DateOnly PeriodTill);
record CreateCoverageRequest(Guid ScheduledShiftId, string Anledning);
record TilldelaCoverageRequest(Guid AnstallId);
