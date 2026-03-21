using Microsoft.EntityFrameworkCore;
using RegionHR.CaseManagement.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class GrievanceEndpoints
{
    public static WebApplication MapGrievanceEndpoints(this WebApplication app)
    {
        var grievance = app.MapGroup("/api/v1/grievance")
            .WithTags("Klagomål / Grievance")
            .RequireAuthorization();

        // GET — list grievances
        grievance.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var items = await db.Grievances
                .OrderByDescending(g => g.SkapadVid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(items.Select(g => new
            {
                g.Id,
                g.AnstallId,
                Typ = g.Typ.ToString(),
                g.Beskrivning,
                g.FackligRepresentant,
                Status = g.Status.ToString(),
                g.InlamnadVid,
                g.SkapadVid
            }));
        }).WithName("ListGrievances");

        // POST — create grievance
        grievance.MapPost("/", async (CreateGrievanceRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var item = Grievance.Skapa(
                EmployeeId.From(req.AnstallId),
                req.Typ,
                req.Beskrivning,
                req.FackligRepresentant);

            await db.Grievances.AddAsync(item, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/grievance/{item.Id}", new
            {
                Id = item.Id.Value,
                Typ = item.Typ.ToString(),
                Status = item.Status.ToString()
            });
        }).WithName("CreateGrievance");

        // PUT — start investigation
        grievance.MapPut("/{id:guid}/utredning", async (Guid id, StartInvestigationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var item = await db.Grievances
                .Include(g => g.Utredningar)
                .FirstOrDefaultAsync(g => g.Id == GrievanceId.From(id), ct);

            if (item is null) return Results.NotFound();

            try
            {
                // First acknowledge if filed
                if (item.Status == GrievanceStatus.Filed)
                    item.Bekrafta();

                item.StartaUtredning(req.Utredare);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { Id = item.Id.Value, Status = item.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("StartGrievanceInvestigation");

        // PUT — record decision
        grievance.MapPut("/{id:guid}/beslut", async (Guid id, RecordDecisionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var item = await db.Grievances
                .Include(g => g.Forhandlingar)
                .FirstOrDefaultAsync(g => g.Id == GrievanceId.From(id), ct);

            if (item is null) return Results.NotFound();

            try
            {
                item.FattaBeslut(req.Beslut);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { Id = item.Id.Value, Status = item.Status.ToString(), item.Beslut });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("RecordGrievanceDecision");

        return app;
    }
}

record CreateGrievanceRequest(Guid AnstallId, GrievanceType Typ, string Beskrivning, string? FackligRepresentant = null);
record StartInvestigationRequest(string Utredare);
record RecordDecisionRequest(string Beslut);
