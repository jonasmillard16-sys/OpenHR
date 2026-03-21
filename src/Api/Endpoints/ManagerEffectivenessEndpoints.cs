using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Performance.Domain;

namespace RegionHR.Api.Endpoints;

public static class ManagerEffectivenessEndpoints
{
    public static WebApplication MapManagerEffectivenessEndpoints(this WebApplication app)
    {
        var chef = app.MapGroup("/api/v1/chef").WithTags("Manager Effectiveness").RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista 1:1-möten för chef
        // ============================================================

        chef.MapGet("/oneononone", async (Guid? chefId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.OneOnOneMeetings.AsQueryable();
            if (chefId.HasValue)
                query = query.Where(m => m.ChefId == chefId.Value);

            var meetings = await query
                .OrderByDescending(m => m.Datum)
                .Take(100)
                .ToListAsync(ct);

            return Results.Ok(meetings.Select(m => new
            {
                m.Id, m.ChefId, m.AnstallId, m.Datum,
                m.Agenda, m.Anteckningar, m.AtgardsLista,
                Status = m.Status.ToString(), m.SkapadVid
            }));
        }).WithName("ListOneOnOneMeetings");

        // ============================================================
        // Skapa 1:1-möte
        // ============================================================

        chef.MapPost("/oneononone", async (CreateOneOnOneRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var meeting = OneOnOneMeeting.Skapa(req.ChefId, req.AnstallId, req.Datum, req.Agenda);
            await db.OneOnOneMeetings.AddAsync(meeting, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/chef/oneononone", new
            {
                meeting.Id, meeting.ChefId, meeting.AnstallId,
                meeting.Datum, Status = meeting.Status.ToString()
            });
        }).WithName("CreateOneOnOneMeeting");

        // ============================================================
        // Genomför 1:1-möte
        // ============================================================

        chef.MapPost("/oneononone/{id:guid}/genomfor", async (Guid id, GenomforMeetingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var meeting = await db.OneOnOneMeetings.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (meeting is null) return Results.NotFound();

            try
            {
                meeting.Genomfor(req.Anteckningar);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { meeting.Id, Status = meeting.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompleteOneOnOneMeeting");

        // ============================================================
        // Hämta scorecard
        // ============================================================

        chef.MapGet("/scorecard", async (Guid chefId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var scorecard = await db.ManagerScorecards
                .Where(s => s.ChefId == chefId)
                .OrderByDescending(s => s.GenereradVid)
                .FirstOrDefaultAsync(ct);

            if (scorecard is null) return Results.NotFound();

            return Results.Ok(new
            {
                scorecard.Id, scorecard.ChefId, scorecard.Period,
                scorecard.SpanOfControl, scorecard.TeamOmsattning,
                scorecard.EngagementDelta, scorecard.UtvecklingsplanFardiggrad,
                scorecard.MedelTidMellanOneonone, scorecard.GenereradVid
            });
        }).WithName("GetManagerScorecard");

        // ============================================================
        // Lista coaching nudges
        // ============================================================

        chef.MapGet("/nudges", async (Guid chefId, bool? olasta, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.CoachingNudges.Where(n => n.ChefId == chefId);
            if (olasta == true)
                query = query.Where(n => !n.ArLast);

            var nudges = await query
                .OrderByDescending(n => n.SkapadVid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(nudges.Select(n => new
            {
                n.Id, n.ChefId, n.Typ, n.Meddelande, n.ArLast, n.SkapadVid
            }));
        }).WithName("ListCoachingNudges");

        // ============================================================
        // Markera nudge som läst
        // ============================================================

        chef.MapPost("/nudges/{id:guid}/last", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var nudge = await db.CoachingNudges.FirstOrDefaultAsync(n => n.Id == id, ct);
            if (nudge is null) return Results.NotFound();

            nudge.MarkeraSomLast();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { nudge.Id, nudge.ArLast });
        }).WithName("MarkNudgeAsRead");

        return app;
    }
}

// Request DTOs
record CreateOneOnOneRequest(Guid ChefId, Guid AnstallId, DateTime Datum, string? Agenda);
record GenomforMeetingRequest(string Anteckningar);
