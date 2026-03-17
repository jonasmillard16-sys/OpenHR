using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.MinKompetens;

/// <summary>
/// Adapter mot MinKompetens (kompetenshanteringssystem).
/// REST-baserad synkronisering av kompetensdata och gapanalys.
/// </summary>
public sealed class MinKompetensAdapter : IIntegrationAdapter
{
    public string SystemName => "MinKompetens";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "SynkaKompetenser" => await SynkaKompetenser(request, ct),
            "HamtaGapAnalys" => await HamtaGapAnalys(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Synkroniserar kompetensdata till MinKompetens.
    /// Push av anställdas registrerade kompetenser.
    /// </summary>
    private Task<IntegrationResult> SynkaKompetenser(IntegrationRequest request, CancellationToken ct)
    {
        KompetensData? data;
        try
        {
            data = request.Payload is KompetensData k
                ? k
                : JsonSerializer.Deserialize<KompetensData>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för kompetensdata"));
        }

        if (data is null)
            return Task.FromResult(new IntegrationResult(false, "Kompetensdata saknas"));

        // I produktion: POST https://minkompetens.region.se/api/v1/kompetenser
        return Task.FromResult(new IntegrationResult(
            true,
            $"Kompetenser synkade för anställd {data.AnstallId} ({data.Kompetenser.Count} kompetenser)",
            new { Synkad = DateTime.UtcNow, AntalKompetenser = data.Kompetenser.Count }));
    }

    /// <summary>
    /// Hämtar gapanalys från MinKompetens.
    /// Visar vilka kompetenser som saknas eller håller på att gå ut.
    /// </summary>
    private Task<IntegrationResult> HamtaGapAnalys(IntegrationRequest request, CancellationToken ct)
    {
        var enhetId = request.Metadata?.GetValueOrDefault("EnhetId") ?? "E-001";

        // I produktion: GET https://minkompetens.region.se/api/v1/gapanalys?enhet={enhetId}
        // Simulerat svar
        var gapAnalys = new GapAnalysResultat
        {
            EnhetId = enhetId,
            SaknadeKompetenser =
            [
                new Kompetens { Namn = "Tracheostomivård", Niva = "Specialist", UtgarDatum = null },
                new Kompetens { Namn = "Cytostatika-hantering", Niva = "Grund", UtgarDatum = null }
            ],
            UtgandeKompetenser =
            [
                new Kompetens { Namn = "HLR-kompetens", Niva = "C", UtgarDatum = new DateOnly(2026, 6, 30) }
            ]
        };

        return Task.FromResult(new IntegrationResult(
            true,
            $"Gapanalys hämtad för enhet {enhetId}",
            gapAnalys));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: GET https://minkompetens.region.se/api/v1/health
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class KompetensData
{
    public string AnstallId { get; set; } = string.Empty;
    public List<Kompetens> Kompetenser { get; set; } = [];
}

public sealed class Kompetens
{
    public string Namn { get; set; } = string.Empty;
    public string Niva { get; set; } = string.Empty;           // Grund, Avancerad, Specialist, eller t.ex. "C" för HLR
    public DateOnly? UtgarDatum { get; set; }
}

public sealed class GapAnalysResultat
{
    public string EnhetId { get; set; } = string.Empty;
    public List<Kompetens> SaknadeKompetenser { get; set; } = [];
    public List<Kompetens> UtgandeKompetenser { get; set; } = [];
}
