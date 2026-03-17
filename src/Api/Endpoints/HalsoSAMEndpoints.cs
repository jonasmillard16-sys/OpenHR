using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.HalsoSAM.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class HalsoSAMEndpoints
{
    public static WebApplication MapHalsoSAMEndpoints(this WebApplication app)
    {
        var rehab = app.MapGroup("/api/v1/halsosam").WithTags("HälsoSAM").RequireAuthorization("ChefEllerHR");

        rehab.MapGet("/arenden", async (string? status, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.RehabCases.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RehabStatus>(status, true, out var s))
                query = query.Where(r => r.Status == s);

            var result = await query
                .OrderByDescending(r => r.SkapadVid)
                .Take(100)
                .ToListAsync(ct);

            return Results.Ok(result.Select(r => new
            {
                r.Id, r.AnstallId, Trigger = r.Trigger.ToString(),
                Status = r.Status.ToString(), r.SkapadVid, r.ArendeagareHR,
                r.Uppfoljning14Dagar, r.Uppfoljning90Dagar, r.Uppfoljning180Dagar, r.Uppfoljning365Dagar
            }));
        }).WithName("ListRehabCases");

        rehab.MapGet("/arende/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = await db.RehabCases
                .FirstOrDefaultAsync(r => r.Id == id, ct);
            return arende is not null ? Results.Ok(arende) : Results.NotFound();
        }).WithName("GetRehabCase");

        rehab.MapPost("/arende", async (CreateRehabRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = RehabCase.Skapa(EmployeeId.From(req.AnstallId), req.Trigger);
            await db.RehabCases.AddAsync(arende, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/halsosam/arende/{arende.Id}", new { arende.Id });
        }).WithName("CreateRehabCase");

        rehab.MapPost("/arende/{id:guid}/tilldela", async (Guid id, TilldelaRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = await db.RehabCases.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (arende is null) return Results.NotFound();
            arende.TilldelaArendeagare(EmployeeId.From(req.HRPersonId));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { arende.Id, Status = arende.Status.ToString() });
        }).WithName("AssignRehabOwner");

        rehab.MapPost("/arende/{id:guid}/rehabplan", async (Guid id, RehabPlanRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = await db.RehabCases.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (arende is null) return Results.NotFound();
            arende.SattRehabPlan(req.Plan);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { arende.Id, Status = arende.Status.ToString() });
        }).WithName("SetRehabPlan");

        rehab.MapPost("/arende/{id:guid}/anteckning", async (Guid id, AnteckningRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = await db.RehabCases.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (arende is null) return Results.NotFound();
            arende.LaggTillAnteckning(req.Text, EmployeeId.From(req.ForfattareId));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { arende.Id, AntalAnteckningar = arende.Anteckningar.Count });
        }).WithName("AddRehabNote");

        rehab.MapPost("/arende/{id:guid}/avsluta", async (Guid id, AvslutaRehabRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var arende = await db.RehabCases.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (arende is null) return Results.NotFound();
            arende.Avsluta(req.Slutsats);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { arende.Id, Status = arende.Status.ToString() });
        }).WithName("CloseRehabCase");

        rehab.MapGet("/kommande-uppfoljningar", async (int? dagar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var dagarFramat = dagar ?? 14;
            var now = DateTime.UtcNow;
            var cutoff = now.AddDays(dagarFramat);

            var aktiva = await db.RehabCases
                .Where(r => r.Status != RehabStatus.Avslutad)
                .ToListAsync(ct);

            var uppfoljningar = new List<object>();
            foreach (var a in aktiva)
            {
                if (a.Uppfoljning14Dagar.HasValue && a.Uppfoljning14Dagar <= cutoff && a.Uppfoljning14Dagar >= now)
                    uppfoljningar.Add(new { a.Id, a.AnstallId, DagNr = 14, Datum = a.Uppfoljning14Dagar });
                if (a.Uppfoljning90Dagar.HasValue && a.Uppfoljning90Dagar <= cutoff && a.Uppfoljning90Dagar >= now)
                    uppfoljningar.Add(new { a.Id, a.AnstallId, DagNr = 90, Datum = a.Uppfoljning90Dagar });
                if (a.Uppfoljning180Dagar.HasValue && a.Uppfoljning180Dagar <= cutoff && a.Uppfoljning180Dagar >= now)
                    uppfoljningar.Add(new { a.Id, a.AnstallId, DagNr = 180, Datum = a.Uppfoljning180Dagar });
                if (a.Uppfoljning365Dagar.HasValue && a.Uppfoljning365Dagar <= cutoff && a.Uppfoljning365Dagar >= now)
                    uppfoljningar.Add(new { a.Id, a.AnstallId, DagNr = 365, Datum = a.Uppfoljning365Dagar });
            }

            return Results.Ok(new { AntalKommande = uppfoljningar.Count, Uppfoljningar = uppfoljningar });
        }).WithName("GetUpcomingFollowUps");

        return app;
    }
}

record CreateRehabRequest(Guid AnstallId, RehabTrigger Trigger);
record TilldelaRequest(Guid HRPersonId);
record RehabPlanRequest(string Plan);
record AnteckningRequest(string Text, Guid ForfattareId);
record AvslutaRehabRequest(string Slutsats);
