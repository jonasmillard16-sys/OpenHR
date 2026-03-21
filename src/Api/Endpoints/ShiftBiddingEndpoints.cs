using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class ShiftBiddingEndpoints
{
    public static WebApplication MapShiftBiddingEndpoints(this WebApplication app)
    {
        var oppnaPass = app.MapGroup("/api/v1/scheduling/oppna-pass")
            .WithTags("Öppna Pass / Shift Bidding")
            .RequireAuthorization();

        // GET — list open shifts
        oppnaPass.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var shifts = await db.OpenShifts
                .Include(s => s.Bud)
                .Where(s => s.Status == OpenShiftStatus.Published || s.Status == OpenShiftStatus.Bidding)
                .OrderBy(s => s.Datum)
                .ThenBy(s => s.StartTid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(shifts.Select(s => new
            {
                s.Id,
                s.EnhetId,
                s.Datum,
                s.PassTyp,
                s.StartTid,
                s.SlutTid,
                s.KravProfil,
                s.Ersattning,
                Status = s.Status.ToString(),
                AntalBud = s.Bud.Count
            }));
        }).WithName("ListOpenShifts");

        // POST — place bid
        oppnaPass.MapPost("/{id:guid}/bud", async (Guid id, PlaceBidRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var shift = await db.OpenShifts
                .Include(s => s.Bud)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (shift is null) return Results.NotFound();

            if (shift.Status != OpenShiftStatus.Published && shift.Status != OpenShiftStatus.Bidding)
                return Results.BadRequest(new { error = "Passet är inte öppet för budgivning." });

            // Check if employee already has a bid
            if (shift.Bud.Any(b => b.AnstallId == EmployeeId.From(req.AnstallId) && b.Status == ShiftBidStatus.Pending))
                return Results.BadRequest(new { error = "Anställd har redan ett aktivt bud på detta pass." });

            var bud = ShiftBid.Skapa(id, EmployeeId.From(req.AnstallId), req.Prioritet, req.Motivering);
            // Note: bid is linked via FK; status transition handled by DB save
            await db.ShiftBids.AddAsync(bud, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/scheduling/oppna-pass/{id}/bud/{bud.Id}", new
            {
                bud.Id,
                bud.OpenShiftId,
                bud.AnstallId,
                bud.Prioritet,
                Status = bud.Status.ToString()
            });
        }).WithName("PlaceShiftBid");

        // POST — assign (manager)
        oppnaPass.MapPost("/{id:guid}/tilldela", async (Guid id, AssignShiftRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var shift = await db.OpenShifts
                .Include(s => s.Bud)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (shift is null) return Results.NotFound();

            var bud = shift.Bud.Where(b => b.Status == ShiftBidStatus.Pending).ToList();
            if (bud.Count == 0)
                return Results.BadRequest(new { error = "Inga aktiva bud att tilldela." });

            // Get employee info for all bidders
            var anstallIds = bud.Select(b => b.AnstallId.Value).ToList();
            var employments = await db.Employments
                .Where(e => anstallIds.Contains(e.AnstallId.Value))
                .ToListAsync(ct);

            var fatigueScores = await db.FatigueScores
                .Where(f => anstallIds.Contains(f.AnstallId.Value))
                .GroupBy(f => f.AnstallId)
                .Select(g => g.OrderByDescending(f => f.BeraknadVid).First())
                .ToListAsync(ct);

            var scheduledShifts = await db.ScheduledShifts
                .Where(s => anstallIds.Contains(s.AnstallId.Value) && s.Datum == shift.Datum)
                .ToListAsync(ct);

            // Build employee info
            var anstallda = anstallIds.Select(aId =>
            {
                var emp = employments.FirstOrDefault(e => e.AnstallId.Value == aId);
                var fatigue = fatigueScores.FirstOrDefault(f => f.AnstallId.Value == aId);
                var harPass = scheduledShifts.Any(s => s.AnstallId.Value == aId);

                return new ShiftBidAssigner.EmployeeInfo
                {
                    AnstallId = EmployeeId.From(aId),
                    AnstallningsDatum = emp?.Giltighetsperiod.Start ?? DateOnly.FromDateTime(DateTime.Today),
                    Kompetenser = [],
                    AntalExtraPassSenaste30Dagar = 0,
                    FatigueScore = fatigue?.Poang ?? 0,
                    HarPassPaDatum = harPass,
                    SenastePassSlut = null
                };
            }).ToList();

            var assigner = new ShiftBidAssigner();
            var result = assigner.Tilldela(shift, bud, req.Metod, anstallda);

            if (!result.Success)
                return Results.BadRequest(new { error = result.Motivering });

            if (result.BidResult is not null)
                await db.ShiftBidResults.AddAsync(result.BidResult, ct);

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                result.VinnareId,
                result.Metod,
                result.Motivering,
                Status = shift.Status.ToString()
            });
        }).WithName("AssignOpenShift");

        return app;
    }
}

record PlaceBidRequest(Guid AnstallId, int Prioritet = 1, string? Motivering = null);
record AssignShiftRequest(string Metod);
