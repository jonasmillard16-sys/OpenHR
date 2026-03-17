using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Scheduling.Domain;

namespace RegionHR.Api.Endpoints;

public static class TimesheetEndpoints
{
    public static WebApplication MapTimesheetEndpoints(this WebApplication app)
    {
        var tid = app.MapGroup("/api/v1/tidrapporter").WithTags("Tidrapportering").RequireAuthorization();

        tid.MapGet("/", async (Guid? anstallId, int? ar, int? manad, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.Timesheets.AsQueryable();
            if (anstallId.HasValue)
                query = query.Where(t => t.AnstallId == anstallId.Value);
            if (ar.HasValue)
                query = query.Where(t => t.Ar == ar.Value);
            if (manad.HasValue)
                query = query.Where(t => t.Manad == manad.Value);

            var result = await query.OrderByDescending(t => t.Ar).ThenByDescending(t => t.Manad).ToListAsync(ct);
            return Results.Ok(result.Select(t => new
            {
                t.Id, t.AnstallId, t.Ar, t.Manad,
                Status = t.Status.ToString(),
                t.PlaneradeTimmar, t.FaktiskaTimmar, t.Avvikelse, t.Overtid,
                t.GodkandAv, t.GodkandVid, t.SkapadVid, t.Kommentar
            }));
        }).WithName("ListTimesheets");

        tid.MapPost("/", async (CreateTimesheetRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var timesheet = Timesheet.Skapa(req.AnstallId, req.Ar, req.Manad, req.PlaneradeTimmar);
            await db.Timesheets.AddAsync(timesheet, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/tidrapporter/{timesheet.Id}", new
            {
                timesheet.Id, timesheet.AnstallId, timesheet.Ar, timesheet.Manad,
                Status = timesheet.Status.ToString()
            });
        }).WithName("CreateTimesheet");

        tid.MapPost("/{id:guid}/registrera", async (Guid id, RegistreraTimmarRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var timesheet = await db.Timesheets.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (timesheet is null) return Results.NotFound();

            try
            {
                timesheet.RegistreraTimmar(req.Faktiska, req.Overtid);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    timesheet.Id, timesheet.FaktiskaTimmar, timesheet.Overtid, timesheet.Avvikelse,
                    Status = timesheet.Status.ToString()
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("RegisterTimesheetHours");

        tid.MapPost("/{id:guid}/skickain", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var timesheet = await db.Timesheets.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (timesheet is null) return Results.NotFound();

            try
            {
                timesheet.SkickaIn();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { timesheet.Id, Status = timesheet.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SubmitTimesheet");

        tid.MapPost("/{id:guid}/godkann", async (Guid id, GodkannTimesheetRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var timesheet = await db.Timesheets.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (timesheet is null) return Results.NotFound();

            try
            {
                timesheet.Godkann(req.Godkannare, req.Kommentar);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    timesheet.Id, Status = timesheet.Status.ToString(),
                    timesheet.GodkandAv, timesheet.GodkandVid
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("ApproveTimesheet");

        tid.MapPost("/{id:guid}/avvisa", async (Guid id, AvvisaTimesheetRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var timesheet = await db.Timesheets.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (timesheet is null) return Results.NotFound();

            try
            {
                timesheet.Avvisa(req.Godkannare, req.Kommentar);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    timesheet.Id, Status = timesheet.Status.ToString(),
                    timesheet.Kommentar
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("RejectTimesheet");

        return app;
    }
}

// Request DTOs for Timesheet
record CreateTimesheetRequest(Guid AnstallId, int Ar, int Manad, decimal PlaneradeTimmar);
record RegistreraTimmarRequest(decimal Faktiska, decimal Overtid = 0);
record GodkannTimesheetRequest(Guid Godkannare, string? Kommentar = null);
record AvvisaTimesheetRequest(Guid Godkannare, string Kommentar);
