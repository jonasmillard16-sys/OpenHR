using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Platform.Domain;

namespace RegionHR.Api.Endpoints;

public static class PlatformEndpoints
{
    public static WebApplication MapPlatformEndpoints(this WebApplication app)
    {
        // ---- Events ----
        var events = app.MapGroup("/api/v1/platform/events").WithTags("Platform - Events").RequireAuthorization("Systemadmin");

        events.MapGet("/", async (int? sida, int? perSida, string? typ, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.DomainEventRecords.AsQueryable();
            if (!string.IsNullOrWhiteSpace(typ))
                query = query.Where(e => e.Typ == typ);

            var total = await query.CountAsync(ct);
            var pageSize = Math.Clamp(perSida ?? 50, 1, 200);
            var page = Math.Max(sida ?? 1, 1);

            var records = await query
                .OrderByDescending(e => e.SkapadVid)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                sida = page,
                perSida = pageSize,
                totalt = total,
                events = records.Select(e => new
                {
                    e.Id,
                    e.Typ,
                    e.AggregatTyp,
                    e.AggregatId,
                    e.Data,
                    e.KorrelationsId,
                    e.SkapadVid
                })
            });
        }).WithName("ListDomainEvents");

        // ---- Webhooks ----
        var webhooks = app.MapGroup("/api/v1/platform/webhooks").WithTags("Platform - Webhooks").RequireAuthorization("Systemadmin");

        webhooks.MapPost("/", async (CreateWebhookRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var subscription = EventSubscription.Skapa(
                req.Namn,
                req.Url,
                req.HemligNyckel,
                req.EventFilter);

            db.EventSubscriptions.Add(subscription);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/platform/webhooks/{subscription.Id}", new
            {
                subscription.Id,
                subscription.Namn,
                subscription.Url,
                subscription.Status,
                subscription.SkapadVid
            });
        }).WithName("CreateWebhookSubscription");

        webhooks.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var subscriptions = await db.EventSubscriptions
                .OrderByDescending(s => s.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(subscriptions.Select(s => new
            {
                s.Id,
                s.Namn,
                s.Url,
                Status = s.Status.ToString(),
                s.EventFilter,
                s.KonsekutivaMisslyckanden,
                s.SkapadVid
            }));
        }).WithName("ListWebhookSubscriptions");

        webhooks.MapDelete("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var subscription = await db.EventSubscriptions.FindAsync(new object[] { id }, ct);
            if (subscription is null) return Results.NotFound();

            db.EventSubscriptions.Remove(subscription);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("DeleteWebhookSubscription");

        webhooks.MapPost("/{id:guid}/test", async (Guid id, WebhookDeliveryService webhookService, CancellationToken ct) =>
        {
            var (success, statusCode) = await webhookService.TestDeliveryAsync(id, ct);
            return Results.Ok(new { success, httpStatusCode = statusCode });
        }).WithName("TestWebhookSubscription");

        // ---- API Keys ----
        var apiKeys = app.MapGroup("/api/v1/platform/api-nycklar").WithTags("Platform - API-nycklar").RequireAuthorization("Systemadmin");

        apiKeys.MapPost("/", async (CreateApiKeyRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var (entity, plaintextKey) = ApiKey.Skapa(
                req.Namn,
                req.SkapadAv ?? "system",
                req.Scope,
                req.UtgarDatum);

            db.ApiKeys.Add(entity);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/platform/api-nycklar/{entity.Id}", new
            {
                entity.Id,
                entity.Namn,
                entity.NyckelPrefix,
                nyckel = plaintextKey,  // Only shown once!
                entity.Scope,
                entity.UtgarDatum,
                entity.SkapadVid
            });
        }).WithName("CreateApiKey");

        apiKeys.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var keys = await db.ApiKeys
                .OrderByDescending(k => k.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(keys.Select(k => new
            {
                k.Id,
                k.Namn,
                k.NyckelPrefix,
                k.Scope,
                k.UtgarDatum,
                k.SkapadAv,
                k.SkapadVid,
                k.SenastAnvand,
                k.ArAktiv
            }));
        }).WithName("ListApiKeys");

        apiKeys.MapDelete("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var key = await db.ApiKeys.FindAsync(new object[] { id }, ct);
            if (key is null) return Results.NotFound();

            key.Inaktivera();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("InactivateApiKey");

        return app;
    }
}

// Request DTOs for Platform endpoints
record CreateWebhookRequest(string Namn, string Url, string HemligNyckel, string? EventFilter = null);
record CreateApiKeyRequest(string Namn, string? SkapadAv = null, string? Scope = null, DateTime? UtgarDatum = null);
