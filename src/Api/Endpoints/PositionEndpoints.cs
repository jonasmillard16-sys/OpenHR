using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Positions.Domain;

namespace RegionHR.Api.Endpoints;

public static class PositionEndpoints
{
    public static WebApplication MapPositionEndpoints(this WebApplication app)
    {
        var positioner = app.MapGroup("/api/v1/positioner").WithTags("Positioner").RequireAuthorization();

        // ============================================================
        // Lista positioner för enhet
        // ============================================================

        positioner.MapGet("/", async (Guid? enhetId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.Positions_Table.AsQueryable();
            if (enhetId.HasValue)
                query = query.Where(p => p.EnhetId == enhetId.Value);

            var positions = await query.OrderBy(p => p.Titel).ToListAsync(ct);

            return Results.Ok(positions.Select(p => new
            {
                p.Id, p.EnhetId, p.Titel, p.BESTAKod, p.AIDKod,
                Status = p.Status.ToString(),
                p.BudgeteradManadslon, p.Sysselsattningsgrad,
                p.InnehavareAnstallId, p.SkapadVid
            }));
        }).WithName("ListPositions");

        // ============================================================
        // Hämta position
        // ============================================================

        positioner.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions_Table.FirstOrDefaultAsync(p => p.Id == id, ct);
            return position is not null ? Results.Ok(position) : Results.NotFound();
        }).WithName("GetPosition");

        // ============================================================
        // Skapa position
        // ============================================================

        positioner.MapPost("/", async (CreatePositionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = Position.Skapa(req.EnhetId, req.Titel, req.BudgeteradManadslon, req.Sysselsattningsgrad, req.BESTAKod, req.AIDKod);
            await db.Positions_Table.AddAsync(position, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/positioner/{position.Id}", new
            {
                position.Id, position.EnhetId, position.Titel,
                Status = position.Status.ToString()
            });
        }).WithName("CreatePosition");

        // ============================================================
        // Tillsätt position med anställd
        // ============================================================

        positioner.MapPost("/{id:guid}/tillsatt", async (Guid id, TillsattPositionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions_Table.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (position is null) return Results.NotFound();

            try
            {
                position.Tillsatt(req.AnstallId);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { position.Id, Status = position.Status.ToString(), position.InnehavareAnstallId });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("FillPosition");

        // ============================================================
        // Vakansätt position
        // ============================================================

        positioner.MapPost("/{id:guid}/vakansatt", async (Guid id, VakansattRequest? req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions_Table.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (position is null) return Results.NotFound();

            position.Vakansatt(req?.Anledning);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { position.Id, Status = position.Status.ToString() });
        }).WithName("VacatePosition");

        // ============================================================
        // Frys position
        // ============================================================

        positioner.MapPost("/{id:guid}/frys", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions_Table.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (position is null) return Results.NotFound();

            position.Frys();
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { position.Id, Status = position.Status.ToString() });
        }).WithName("FreezePosition");

        // ============================================================
        // Avveckla position
        // ============================================================

        positioner.MapPost("/{id:guid}/avveckla", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions_Table.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (position is null) return Results.NotFound();

            position.Avveckla();
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { position.Id, Status = position.Status.ToString(), position.AvveckladVid });
        }).WithName("DecommissionPosition");

        // ============================================================
        // Lista headcount-planer
        // ============================================================

        var headcount = app.MapGroup("/api/v1/headcount").WithTags("Headcount").RequireAuthorization();

        headcount.MapGet("/", async (Guid? enhetId, int? ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.HeadcountPlans.AsQueryable();
            if (enhetId.HasValue) query = query.Where(h => h.EnhetId == enhetId.Value);
            if (ar.HasValue) query = query.Where(h => h.Ar == ar.Value);

            var plans = await query.OrderByDescending(h => h.Ar).ToListAsync(ct);

            return Results.Ok(plans.Select(h => new
            {
                h.Id, h.EnhetId, h.Ar,
                h.BudgeteradePositioner, h.BudgeteradFTE, h.BudgeteradKostnad,
                h.FaktiskaPositioner, h.FaktiskFTE, h.FaktiskKostnad,
                h.Avvikelse
            }));
        }).WithName("ListHeadcountPlans");

        // ============================================================
        // Skapa headcount-plan
        // ============================================================

        headcount.MapPost("/", async (CreateHeadcountPlanRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var plan = HeadcountPlan.Skapa(req.EnhetId, req.Ar, req.BudgeteradePositioner, req.BudgeteradFTE, req.BudgeteradKostnad);
            await db.HeadcountPlans.AddAsync(plan, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/headcount/{plan.Id}", new
            {
                plan.Id, plan.EnhetId, plan.Ar,
                plan.BudgeteradePositioner, plan.BudgeteradFTE, plan.BudgeteradKostnad
            });
        }).WithName("CreateHeadcountPlan");

        return app;
    }
}

// Request DTOs
record CreatePositionRequest(Guid EnhetId, string Titel, decimal BudgeteradManadslon, decimal Sysselsattningsgrad, string? BESTAKod = null, string? AIDKod = null);
record TillsattPositionRequest(Guid AnstallId);
record VakansattRequest(string? Anledning = null);
record CreateHeadcountPlanRequest(Guid EnhetId, int Ar, int BudgeteradePositioner, decimal BudgeteradFTE, decimal BudgeteradKostnad);
