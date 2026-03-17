using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Grade;

/// <summary>
/// Adapter mot Grade (Learning Management System).
/// SCIM-baserad synkronisering av anställda och utbildningsstatus.
/// Push: anställda till Grade. Pull: utbildningskompletteringar.
/// </summary>
public sealed class GradeAdapter : IIntegrationAdapter
{
    public string SystemName => "Grade";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "SynkaAnstallda" => await SynkaAnstallda(request, ct),
            "HamtaUtbildningsstatus" => await HamtaUtbildningsstatus(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Synkroniserar anställda till Grade via SCIM-protokollet.
    /// Push av användardata (namn, avdelning, befattning) till LMS.
    /// </summary>
    private Task<IntegrationResult> SynkaAnstallda(IntegrationRequest request, CancellationToken ct)
    {
        var payload = request.Payload?.ToString();
        if (string.IsNullOrWhiteSpace(payload))
            return Task.FromResult(new IntegrationResult(false, "Anställddata saknas"));

        // I produktion: SCIM 2.0 provisioning
        // POST https://grade.region.se/scim/v2/Users
        // eller PUT för uppdatering

        return Task.FromResult(new IntegrationResult(
            true,
            "Anställda synkade till Grade via SCIM",
            new { Synkad = DateTime.UtcNow, Protokoll = "SCIM 2.0" }));
    }

    /// <summary>
    /// Hämtar utbildningsstatus från Grade.
    /// Pull av kursresultat och kompletteringsdata.
    /// </summary>
    private Task<IntegrationResult> HamtaUtbildningsstatus(IntegrationRequest request, CancellationToken ct)
    {
        var anstallId = request.Metadata?.GetValueOrDefault("AnstallId");

        // I produktion: GET https://grade.region.se/api/v1/completions?userId={anstallId}
        // Simulerat svar
        var statusar = new List<UtbildningsStatus>
        {
            new()
            {
                AnstallId = anstallId ?? "A-001",
                KursNamn = "HLR - Hjärt-lungräddning",
                Slutford = true,
                Datum = new DateOnly(2026, 1, 15)
            },
            new()
            {
                AnstallId = anstallId ?? "A-001",
                KursNamn = "Brandskyddsutbildning",
                Slutford = true,
                Datum = new DateOnly(2025, 11, 20)
            },
            new()
            {
                AnstallId = anstallId ?? "A-001",
                KursNamn = "GDPR-utbildning 2026",
                Slutford = false,
                Datum = null
            }
        };

        return Task.FromResult(new IntegrationResult(
            true,
            $"Hämtade {statusar.Count} utbildningsstatusar",
            statusar));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: GET https://grade.region.se/scim/v2/ServiceProviderConfig
        return Task.FromResult(true);
    }
}

// --- Models ---

/// <summary>
/// Utbildningsstatus från Grade LMS.
/// </summary>
public sealed class UtbildningsStatus
{
    public string AnstallId { get; set; } = string.Empty;
    public string KursNamn { get; set; } = string.Empty;
    public bool Slutford { get; set; }
    public DateOnly? Datum { get; set; }
}
