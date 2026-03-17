using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Microweb;

/// <summary>
/// Adapter mot Microweb (personalaktsarkiv).
/// Hanterar arkivering och hämtning av personalhandlingar med GDPR-klassificering.
/// Bevarandetider per dokumenttyp enligt regionens dokumenthanteringsplan.
/// </summary>
public sealed class MicrowebArkivAdapter : IIntegrationAdapter
{
    public string SystemName => "Microweb";

    /// <summary>
    /// Bevarandetider per dokumenttyp (i år).
    /// Baserat på regionens dokumenthanteringsplan och lagkrav.
    /// </summary>
    internal static readonly Dictionary<string, int> Bevarandetider = new()
    {
        ["Loneunderlag"] = 7,           // Bokföringslagen 7:2
        ["Anstallningsavtal"] = 10,     // 10 år efter anställningens slut
        ["Arbetsskadeanmalan"] = 10,    // Arbetsmiljöverkets krav
        ["Rehabiliteringsplan"] = 10,   // Kopplat till arbetsskada
        ["Lonespecifikation"] = 7,      // Bokföringslagen
        ["Intyg"] = 5,                  // Allmän bevarandetid
        ["Bedomning"] = 2,             // Rekryteringshandlingar
        ["Varning"] = 5,               // Arbetsrättslig praxis
        ["Uppsagning"] = 10,           // Preskriptionstid
        ["Sjukintyg"] = 2,            // Känsliga personuppgifter, kort bevarande
    };

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "ArkiveraHandling" => await ArkiveraHandling(request, ct),
            "HamtaHandling" => await HamtaHandling(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Arkiverar en personalhandling i Microweb med korrekt GDPR-klassificering och bevarandetid.
    /// </summary>
    private Task<IntegrationResult> ArkiveraHandling(IntegrationRequest request, CancellationToken ct)
    {
        ArkivHandling? handling;
        try
        {
            handling = request.Payload is ArkivHandling a
                ? a
                : JsonSerializer.Deserialize<ArkivHandling>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för arkivhandling"));
        }

        if (handling is null)
            return Task.FromResult(new IntegrationResult(false, "Arkivhandling saknas"));

        if (string.IsNullOrWhiteSpace(handling.AnstallId))
            return Task.FromResult(new IntegrationResult(false, "AnställdID saknas"));

        if (string.IsNullOrWhiteSpace(handling.Typ))
            return Task.FromResult(new IntegrationResult(false, "Dokumenttyp saknas"));

        // Sätt bevarandetid automatiskt om inte angiven
        if (handling.Bevarandetid <= 0)
        {
            handling.Bevarandetid = Bevarandetider.GetValueOrDefault(handling.Typ, 5);
        }

        // Validera GDPR-klass
        if (string.IsNullOrWhiteSpace(handling.GDPRKlass))
        {
            handling.GDPRKlass = BestamGDPRKlass(handling.Typ);
        }

        // I produktion: POST till Microweb REST API
        var dokumentId = handling.DokumentId ?? $"MW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";

        return Task.FromResult(new IntegrationResult(
            true,
            $"Handling arkiverad: {dokumentId} (typ: {handling.Typ}, GDPR: {handling.GDPRKlass}, bevara: {handling.Bevarandetid} år)",
            new { DokumentId = dokumentId, handling.Typ, handling.GDPRKlass, handling.Bevarandetid }));
    }

    /// <summary>
    /// Hämtar en personalhandling från Microweb med behörighetskontroll.
    /// </summary>
    private Task<IntegrationResult> HamtaHandling(IntegrationRequest request, CancellationToken ct)
    {
        var dokumentId = request.Metadata?.GetValueOrDefault("DokumentId");
        if (string.IsNullOrWhiteSpace(dokumentId))
            return Task.FromResult(new IntegrationResult(false, "DokumentId saknas"));

        // I produktion: GET https://microweb.region.se/api/v1/dokument/{dokumentId}
        // med behörighetskontroll
        var handling = new ArkivHandling
        {
            DokumentId = dokumentId,
            Typ = "Anstallningsavtal",
            AnstallId = "A-001",
            GDPRKlass = "Konfidentiell",
            Bevarandetid = 10,
            Innehall = "[Dokumentinnehåll - krypterat i produktion]"
        };

        return Task.FromResult(new IntegrationResult(
            true,
            $"Handling hämtad: {dokumentId}",
            handling));
    }

    /// <summary>
    /// Bestämmer GDPR-klass baserat på dokumenttyp.
    /// </summary>
    internal static string BestamGDPRKlass(string dokumenttyp) => dokumenttyp switch
    {
        "Sjukintyg" => "Känslig",               // Hälsouppgifter = känsliga personuppgifter (Art. 9 GDPR)
        "Rehabiliteringsplan" => "Känslig",
        "Arbetsskadeanmalan" => "Känslig",
        "Loneunderlag" => "Konfidentiell",
        "Lonespecifikation" => "Konfidentiell",
        "Anstallningsavtal" => "Konfidentiell",
        _ => "Intern"
    };

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: GET https://microweb.region.se/api/v1/health
        return Task.FromResult(true);
    }
}

// --- Models ---

/// <summary>
/// Personalhandling för arkivering i Microweb.
/// </summary>
public sealed class ArkivHandling
{
    public string? DokumentId { get; set; }
    public string Typ { get; set; } = string.Empty;           // Loneunderlag, Anstallningsavtal, etc.
    public string AnstallId { get; set; } = string.Empty;
    public string GDPRKlass { get; set; } = string.Empty;     // Känslig, Konfidentiell, Intern
    public int Bevarandetid { get; set; }                      // År
    public string? Innehall { get; set; }
}
