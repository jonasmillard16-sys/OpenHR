using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Audit.Domain;

namespace RegionHR.Api.Endpoints;

public static class AuditEndpoints
{
    public static WebApplication MapAuditEndpoints(this WebApplication app)
    {
        var audit = app.MapGroup("/api/v1/audit").WithTags("Granskningslogg").RequireAuthorization("Systemadmin");

        audit.MapGet("/", async (string? entityType, string? entityId, int take, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.AuditEntries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(a => a.EntityType == entityType);
            if (!string.IsNullOrWhiteSpace(entityId))
                query = query.Where(a => a.EntityId == entityId);
            var entries = await query.OrderByDescending(a => a.Timestamp).Take(take > 0 ? take : 50).ToListAsync(ct);
            return Results.Ok(entries);
        }).WithName("ListAuditEntries");

        return app;
    }
}
