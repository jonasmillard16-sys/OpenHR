using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Configuration.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Applicerar ett tillägg (Extension) baserat på dess manifestinnehåll.
/// Skapar CustomObjectDefinitions och EventSubscriptions om de inte redan finns.
/// </summary>
public class PluginApplicator
{
    private readonly RegionHRDbContext _db;
    private readonly ILogger<PluginApplicator> _logger;

    public PluginApplicator(RegionHRDbContext db, ILogger<PluginApplicator> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Applicerar tilläggets manifest och spårar vad som skapades i installationens konfiguration.
    /// </summary>
    public async Task AppliceraAsync(
        Extension extension,
        ExtensionInstallation installation,
        CancellationToken ct = default)
    {
        var tillagdaObjekt = new List<string>();
        var tillagdaWebhooks = new List<string>();

        ManifestData manifest;
        try
        {
            manifest = ParseaManifest(extension.Innehall);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Kunde inte tolka manifest för tillägg '{Namn}' (Id: {Id})",
                extension.Namn, extension.Id);
            return;
        }

        // Skapa CustomObjectDefinitions som inte redan finns
        foreach (var co in manifest.CustomObjects)
        {
            if (string.IsNullOrWhiteSpace(co.Namn)) continue;

            var finnsBefintlig = await _db.CustomObjects
                .AnyAsync(o => o.Namn == co.Namn, ct);

            if (!finnsBefintlig)
            {
                var nyttObjekt = CustomObject.Skapa(
                    namn: co.Namn,
                    pluralNamn: co.PluralNamn ?? $"{co.Namn}s",
                    beskrivning: co.Beskrivning ?? $"Skapad av tillägg: {extension.Namn}",
                    faltSchema: co.FaltSchema ?? "[]",
                    ikon: co.Ikon);

                await _db.CustomObjects.AddAsync(nyttObjekt, ct);
                tillagdaObjekt.Add(co.Namn);
                _logger.LogInformation(
                    "Skapade CustomObject '{Namn}' från tillägg '{Extension}'",
                    co.Namn, extension.Namn);
            }
            else
            {
                _logger.LogDebug(
                    "CustomObject '{Namn}' finns redan, hoppar över",
                    co.Namn);
            }
        }

        // Skapa EventSubscriptions (webhooks) som inte redan finns
        foreach (var webhook in manifest.Webhooks)
        {
            if (string.IsNullOrWhiteSpace(webhook.Url)) continue;

            var finnsBefintlig = await _db.EventSubscriptions
                .AnyAsync(s => s.Url == webhook.Url, ct);

            if (!finnsBefintlig)
            {
                var hemligNyckel = GenerateWebhookSecret();
                var subscription = EventSubscription.Skapa(
                    namn: webhook.Namn ?? $"Webhook från {extension.Namn}",
                    url: webhook.Url,
                    hemligNyckel: hemligNyckel,
                    eventFilter: webhook.EventFilter);

                await _db.EventSubscriptions.AddAsync(subscription, ct);
                tillagdaWebhooks.Add(webhook.Url);
                _logger.LogInformation(
                    "Skapade EventSubscription '{Url}' från tillägg '{Extension}'",
                    webhook.Url, extension.Namn);
            }
            else
            {
                _logger.LogDebug(
                    "EventSubscription för '{Url}' finns redan, hoppar över",
                    webhook.Url);
            }
        }

        // Spara allt
        await _db.SaveChangesAsync(ct);

        // Uppdatera installationens metadata med vad som applicerades
        var metadata = JsonSerializer.Serialize(new
        {
            AppliceraddVid = DateTime.UtcNow,
            TillagdaCustomObjects = tillagdaObjekt,
            TillagdaWebhooks = tillagdaWebhooks,
            TotaltCustomObjects = manifest.CustomObjects.Count,
            TotaltWebhooks = manifest.Webhooks.Count
        });

        // ExtensionInstallation.Konfiguration uppdateras via reflection (private setter)
        var konfigProp = typeof(ExtensionInstallation)
            .GetProperty("Konfiguration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        konfigProp?.SetValue(installation, metadata);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tillägg '{Namn}' applicerat: {Objekt} objekt, {Webhooks} webhooks skapade.",
            extension.Namn, tillagdaObjekt.Count, tillagdaWebhooks.Count);
    }

    private static ManifestData ParseaManifest(string innehallJson)
    {
        if (string.IsNullOrWhiteSpace(innehallJson) || innehallJson == "{}")
            return new ManifestData([], []);

        using var doc = JsonDocument.Parse(innehallJson);
        var root = doc.RootElement;

        var customObjects = new List<ManifestCustomObject>();
        if (root.TryGetProperty("customObjects", out var coArray) &&
            coArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in coArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                customObjects.Add(new ManifestCustomObject(
                    Namn: item.TryGetProperty("name", out var n) ? n.GetString() : null,
                    PluralNamn: item.TryGetProperty("pluralName", out var pn) ? pn.GetString() : null,
                    Beskrivning: item.TryGetProperty("description", out var b) ? b.GetString() : null,
                    FaltSchema: item.TryGetProperty("fieldSchema", out var fs) ? fs.GetRawText() : null,
                    Ikon: item.TryGetProperty("icon", out var ik) ? ik.GetString() : null
                ));
            }
        }

        var webhooks = new List<ManifestWebhook>();
        if (root.TryGetProperty("webhooks", out var whArray) &&
            whArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in whArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                webhooks.Add(new ManifestWebhook(
                    Namn: item.TryGetProperty("name", out var n) ? n.GetString() : null,
                    Url: item.TryGetProperty("url", out var u) ? u.GetString() : null,
                    EventFilter: item.TryGetProperty("eventFilter", out var ef) ? ef.GetRawText() : null
                ));
            }
        }

        return new ManifestData(customObjects, webhooks);
    }

    private static string GenerateWebhookSecret()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private record ManifestData(
        List<ManifestCustomObject> CustomObjects,
        List<ManifestWebhook> Webhooks);

    private record ManifestCustomObject(
        string? Namn,
        string? PluralNamn,
        string? Beskrivning,
        string? FaltSchema,
        string? Ikon);

    private record ManifestWebhook(
        string? Namn,
        string? Url,
        string? EventFilter);
}
