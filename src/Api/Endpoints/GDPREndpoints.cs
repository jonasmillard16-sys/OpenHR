using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.GDPR;
using RegionHR.GDPR.Domain;

namespace RegionHR.Api.Endpoints;

public static class GDPREndpoints
{
    public static WebApplication MapGDPREndpoints(this WebApplication app)
    {
        var gdpr = app.MapGroup("/api/v1/gdpr").WithTags("GDPR").RequireAuthorization("Systemadmin");

        // ============================================================
        // Lista begäran om registrerades rättigheter
        // ============================================================

        gdpr.MapGet("/begaran", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var requests = await db.DataSubjectRequests
                .OrderByDescending(r => r.Mottagen)
                .ToListAsync(ct);

            return Results.Ok(requests.Select(r => new
            {
                r.Id, r.AnstallId,
                Typ = r.Typ.ToString(),
                Status = r.Status.ToString(),
                r.Mottagen, r.Deadline, r.SlutfordVid,
                r.HandlaggarId, r.ArForsenad
            }));
        }).WithName("ListDataSubjectRequests");

        // ============================================================
        // Skapa begäran
        // ============================================================

        gdpr.MapPost("/begaran", async (CreateDataSubjectRequestDto req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<RequestType>(req.Typ, true, out var typ))
                return Results.BadRequest(new { error = $"Ogiltig typ: {req.Typ}. Giltiga värden: {string.Join(", ", Enum.GetNames<RequestType>())}" });

            var request = DataSubjectRequest.Skapa(req.AnstallId, typ);
            await db.DataSubjectRequests.AddAsync(request, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/gdpr/begaran/{request.Id}", new
            {
                request.Id, request.AnstallId,
                Typ = request.Typ.ToString(),
                Status = request.Status.ToString(),
                request.Mottagen, request.Deadline
            });
        }).WithName("CreateDataSubjectRequest");

        // ============================================================
        // Tilldela handläggare
        // ============================================================

        gdpr.MapPost("/begaran/{id:guid}/tilldela", async (Guid id, TilldelaHandlaggarRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.DataSubjectRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            request.Tilldela(req.HandlaggarId);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                request.Id,
                Status = request.Status.ToString(),
                request.HandlaggarId
            });
        }).WithName("AssignDataSubjectRequestHandler");

        // ============================================================
        // Slutför begäran
        // ============================================================

        gdpr.MapPost("/begaran/{id:guid}/slutfor", async (Guid id, SlutforBegaranRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.DataSubjectRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            try
            {
                request.Slutfor(req.FilSokvag);
                await db.SaveChangesAsync(ct);

                return Results.Ok(new
                {
                    request.Id,
                    Status = request.Status.ToString(),
                    request.SlutfordVid, request.ResultatFilSokvag
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompleteDataSubjectRequest");

        // ============================================================
        // Generera registerutdrag (GDPR Art 15)
        // ============================================================

        gdpr.MapGet("/registerutdrag/{anstallId:guid}", async (Guid anstallId, RegisterutdragGenerator generator, CancellationToken ct) =>
        {
            var json = await generator.GenerateAsync(anstallId, ct);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return Results.File(bytes, "application/json", $"registerutdrag_{anstallId}_{DateTime.UtcNow:yyyyMMdd}.json");
        }).WithName("GenerateRegisterutdrag");

        // ============================================================
        // Lista utgångna retention-poster
        // ============================================================

        gdpr.MapGet("/retention", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var expired = await db.RetentionRecords
                .Where(r => !r.IsAnonymized && r.RetentionExpires < DateTime.UtcNow)
                .OrderBy(r => r.RetentionExpires)
                .ToListAsync(ct);

            return Results.Ok(expired);
        }).WithName("ListExpiredRetentionRecords");

        // ============================================================
        // Anonymisera retention-post
        // ============================================================

        gdpr.MapPost("/retention/{id:guid}/anonymisera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var record = await db.RetentionRecords.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (record is null) return Results.NotFound();

            record.Anonymize();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { record.Id, record.IsAnonymized, record.AnonymizedAt });
        }).WithName("AnonymizeRetentionRecord");

        return app;
    }
}

// Request DTOs
record CreateDataSubjectRequestDto(Guid AnstallId, string Typ);
record TilldelaHandlaggarRequest(string HandlaggarId);
record SlutforBegaranRequest(string? FilSokvag);
