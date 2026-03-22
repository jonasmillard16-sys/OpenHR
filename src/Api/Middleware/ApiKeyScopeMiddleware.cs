using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Platform.Domain;

namespace RegionHR.Api.Middleware;

/// <summary>
/// Kontrollerar API-nyckelns scope för /api/-rutter.
/// Om X-API-Key header saknas lämnas anropet vidare till vanlig autentisering.
/// </summary>
public class ApiKeyScopeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyScopeMiddleware> _logger;

    public ApiKeyScopeMiddleware(RequestDelegate next, ILogger<ApiKeyScopeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RegionHRDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Kontrollera bara /api/-rutter
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Om ingen X-API-Key header — låt annan autentisering hantera det
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader) ||
            string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            await _next(context);
            return;
        }

        var plaintextKey = apiKeyHeader.ToString().Trim();

        // Hitta matchande nyckel via hash-jämförelse
        var keyHash = ApiKey.HashKey(plaintextKey);
        var apiKey = await db.ApiKeys
            .FirstOrDefaultAsync(k => k.NyckelHash == keyHash, context.RequestAborted);

        if (apiKey is null || !apiKey.ArGiltig())
        {
            _logger.LogWarning("Ogiltig API-nyckel använd för {Path}", path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"fel":"Ogiltig eller utgången API-nyckel"}""",
                context.RequestAborted);
            return;
        }

        // Kontrollera scope
        if (!HarScope(apiKey.Scope, path))
        {
            _logger.LogWarning(
                "API-nyckel '{Namn}' saknar scope för {Path}",
                apiKey.Namn, path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"fel":"API-nyckeln saknar behörighet för denna resurs"}""",
                context.RequestAborted);
            return;
        }

        // Uppdatera senast-använd och fortsätt
        apiKey.UppdateraSenastAnvand();
        await db.SaveChangesAsync(context.RequestAborted);

        await _next(context);
    }

    /// <summary>
    /// Kontrollerar om sökvägen matchar något scope i nyckelns scope-JSON.
    /// Scope är ett JSON-objekt eller array med sökvägsprefix, t.ex.:
    ///   {"scopes":["anstallda","lon"]}  eller  ["anstallda","lon"]
    /// Tom/odefinierad scope ({} eller []) ger åtkomst till allt.
    /// </summary>
    private static bool HarScope(string scopeJson, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(scopeJson) ||
            scopeJson == "{}" ||
            scopeJson == "[]")
        {
            return true; // Ingen scope-begränsning
        }

        try
        {
            using var doc = JsonDocument.Parse(scopeJson);
            var root = doc.RootElement;

            // Stöd för array-format: ["anstallda", "lon"]
            if (root.ValueKind == JsonValueKind.Array)
            {
                return MatcharNagonScope(root.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()!), requestPath);
            }

            // Stöd för objekt-format: {"scopes": ["anstallda", "lon"]}
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("scopes", out var scopesElement) &&
                scopesElement.ValueKind == JsonValueKind.Array)
            {
                return MatcharNagonScope(scopesElement.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()!), requestPath);
            }

            // Okänt format — tillåt inte
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool MatcharNagonScope(IEnumerable<string> scopes, string requestPath)
    {
        var normalizedPath = requestPath.ToLowerInvariant();
        foreach (var scope in scopes)
        {
            if (string.IsNullOrWhiteSpace(scope)) continue;
            var normalizedScope = scope.ToLowerInvariant().Trim('/');
            if (normalizedPath.Contains(normalizedScope, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
