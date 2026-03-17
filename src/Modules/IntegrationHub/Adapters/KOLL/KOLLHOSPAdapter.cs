using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.KOLL;

/// <summary>
/// Adapter för KOLL/HOSP (Socialstyrelsen).
/// Verifierar legitimationer och specialiseringar för hälso- och sjukvårdspersonal.
/// I produktion: REST API mot Socialstyrelsens HOSP-register.
/// </summary>
public sealed class KOLLHOSPAdapter : IIntegrationAdapter
{
    public string SystemName => "KOLL/HOSP";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "VerifyLegitimation" => await VerifyLegitimationAsync(request, ct),
            "GetSpecializations" => await GetSpecializationsAsync(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    private Task<IntegrationResult> VerifyLegitimationAsync(IntegrationRequest request, CancellationToken ct)
    {
        // I produktion: HTTP-anrop till Socialstyrelsens HOSP-API
        // POST https://hosp.socialstyrelsen.se/api/v1/legitimation/verify
        // Body: { "personnummer": "YYYYMMDDNNNN" }

        var personnummer = request.Payload?.ToString();
        if (string.IsNullOrWhiteSpace(personnummer))
            return Task.FromResult(new IntegrationResult(false, "Personnummer saknas"));

        // Simulerat svar
        var result = new LegitimationVerification
        {
            Personnummer = personnummer,
            HarLegitimation = true,
            Yrkestitel = "Sjuksköterska",
            LegitimationsDatum = new DateOnly(2015, 6, 15),
            Status = "Aktiv",
            Specialiseringar = ["Intensivvård", "Anestesisjukvård"],
            SenastVerifierad = DateTime.UtcNow
        };

        return Task.FromResult(new IntegrationResult(true, "Legitimation verifierad", result));
    }

    private Task<IntegrationResult> GetSpecializationsAsync(IntegrationRequest request, CancellationToken ct)
    {
        var result = new[]
        {
            new Specialization("Intensivvård", "Aktiv", new DateOnly(2018, 3, 1)),
            new Specialization("Anestesisjukvård", "Aktiv", new DateOnly(2020, 9, 15))
        };

        return Task.FromResult(new IntegrationResult(true, "Specialiseringar hämtade", result));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: ping Socialstyrelsens API
        return Task.FromResult(true);
    }
}

public sealed class LegitimationVerification
{
    public string Personnummer { get; set; } = string.Empty;
    public bool HarLegitimation { get; set; }
    public string? Yrkestitel { get; set; }
    public DateOnly? LegitimationsDatum { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Specialiseringar { get; set; } = [];
    public DateTime SenastVerifierad { get; set; }
}

public sealed record Specialization(string Namn, string Status, DateOnly Datum);
