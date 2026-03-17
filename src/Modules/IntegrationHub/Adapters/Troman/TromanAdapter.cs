using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Troman;

/// <summary>
/// Adapter mot Troman (förtroendevaldasystem).
/// REST-baserad synkronisering av förtroendevaldas arvoden och anställda.
/// </summary>
public sealed class TromanAdapter : IIntegrationAdapter
{
    public string SystemName => "Troman";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "HamtaArvoden" => await HamtaArvoden(request, ct),
            "SkickaAnstallda" => await SkickaAnstallda(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Hämtar arvoden för förtroendevalda från Troman via REST API.
    /// </summary>
    private Task<IntegrationResult> HamtaArvoden(IntegrationRequest request, CancellationToken ct)
    {
        var period = request.Metadata?.GetValueOrDefault("Period") ?? DateTime.UtcNow.ToString("yyyyMM");

        // I produktion: GET https://troman.region.se/api/v1/arvoden?period={period}
        // Simulerat svar
        var arvoden = new List<Arvode>
        {
            new() { FortroendevaldId = "FV-001", Typ = "Sammanträdesarvode", Belopp = 3500m, Period = period },
            new() { FortroendevaldId = "FV-002", Typ = "Årsarvode", Belopp = 45000m, Period = period }
        };

        return Task.FromResult(new IntegrationResult(
            true,
            $"Hämtade {arvoden.Count} arvoden för period {period}",
            arvoden));
    }

    /// <summary>
    /// Skickar anställningsinformation till Troman för koppling till förtroendeuppdrag.
    /// </summary>
    private Task<IntegrationResult> SkickaAnstallda(IntegrationRequest request, CancellationToken ct)
    {
        // I produktion: POST https://troman.region.se/api/v1/anstallda (batch)
        // request.Payload innehåller lista av anställda
        var payload = request.Payload?.ToString();
        if (string.IsNullOrWhiteSpace(payload))
            return Task.FromResult(new IntegrationResult(false, "Anställddata saknas"));

        return Task.FromResult(new IntegrationResult(
            true,
            "Anställda skickade till Troman",
            new { Synkad = DateTime.UtcNow }));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: GET https://troman.region.se/api/v1/health
        return Task.FromResult(true);
    }
}

// --- Models ---

/// <summary>
/// Arvode för förtroendevald i Troman.
/// </summary>
public sealed class Arvode
{
    public string FortroendevaldId { get; set; } = string.Empty;
    public string Typ { get; set; } = string.Empty;       // Sammanträdesarvode, Årsarvode, etc.
    public decimal Belopp { get; set; }
    public string Period { get; set; } = string.Empty;     // "YYYYMM"
}
