using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Authorization;

namespace RegionHR.Api.Endpoints;

public static class PermissionEndpoints
{
    public static WebApplication MapPermissionEndpoints(this WebApplication app)
    {
        var behorighet = app.MapGroup("/api/v1/behorighet").WithTags("Behörighet").RequireAuthorization("Systemadmin");

        // ============================================================
        // Lista fältregler (field permissions)
        // ============================================================

        behorighet.MapGet("/faltregler", async (string? roll, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.FieldPermissions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(roll))
                query = query.Where(fp => fp.Roll == roll);

            var rules = await query.OrderBy(fp => fp.EntityType).ThenBy(fp => fp.FieldName).ToListAsync(ct);

            return Results.Ok(rules.Select(fp => new
            {
                fp.Id, fp.Roll, fp.EntityType, fp.FieldName,
                AccessLevel = fp.AccessLevel.ToString()
            }));
        }).WithName("ListFieldPermissions");

        // ============================================================
        // Skapa fältregel
        // ============================================================

        behorighet.MapPost("/faltregel", async (CreateFieldPermissionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<FieldAccessLevel>(req.AccessLevel, true, out var level))
                return Results.BadRequest(new { error = $"Ogiltig AccessLevel: {req.AccessLevel}. Giltiga värden: {string.Join(", ", Enum.GetNames<FieldAccessLevel>())}" });

            var permission = FieldPermission.Skapa(req.Roll, req.EntityType, req.FieldName, level);
            await db.FieldPermissions.AddAsync(permission, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/behorighet/faltregler", new
            {
                permission.Id, permission.Roll, permission.EntityType,
                permission.FieldName, AccessLevel = permission.AccessLevel.ToString()
            });
        }).WithName("CreateFieldPermission");

        // ============================================================
        // Lista delegeringar
        // ============================================================

        behorighet.MapGet("/delegeringar", async (Guid? delegatId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.DelegatedAccesses.AsQueryable();
            if (delegatId.HasValue)
                query = query.Where(d => d.DelegatId == delegatId.Value);

            var delegations = await query.OrderByDescending(d => d.SkapadVid).ToListAsync(ct);

            return Results.Ok(delegations.Select(d => new
            {
                d.Id, d.DelegatorId, d.DelegatId, d.Roll,
                d.FranDatum, d.TillDatum, d.Anledning, d.ArAktiv
            }));
        }).WithName("ListDelegations");

        // ============================================================
        // Skapa delegering
        // ============================================================

        behorighet.MapPost("/delegering", async (CreateDelegationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var delegation = DelegatedAccess.Skapa(
                req.DelegatorId, req.DelegatId, req.Roll,
                req.FranDatum, req.TillDatum, req.Anledning);

            await db.DelegatedAccesses.AddAsync(delegation, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/behorighet/delegeringar", new
            {
                delegation.Id, delegation.DelegatorId, delegation.DelegatId,
                delegation.Roll, delegation.FranDatum, delegation.TillDatum
            });
        }).WithName("CreateDelegation");

        // ============================================================
        // Avsluta delegering
        // ============================================================

        behorighet.MapPost("/delegering/{id:guid}/avsluta", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var delegation = await db.DelegatedAccesses.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (delegation is null) return Results.NotFound();

            delegation.Avsluta();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { delegation.Id, delegation.ArAktiv, meddelande = "Delegering avslutad" });
        }).WithName("EndDelegation");

        return app;
    }
}

// Request DTOs
record CreateFieldPermissionRequest(string Roll, string EntityType, string FieldName, string AccessLevel);
record CreateDelegationRequest(Guid DelegatorId, Guid DelegatId, string Roll, DateOnly FranDatum, DateOnly TillDatum, string? Anledning);
