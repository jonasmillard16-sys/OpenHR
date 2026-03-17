using System.Globalization;
using System.Text;
using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.SKR;

/// <summary>
/// Adapter mot Sveriges Kommuner och Regioner (SKR).
/// Genererar årlig personalstatistik per BESTA/AID-kod.
/// Skickas i november varje år.
/// Format: SKR-specifikt filformat med fasta positioner.
/// </summary>
public sealed class SKRStatistikAdapter : IIntegrationAdapter
{
    public string SystemName => "SKR";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraArsstatistik" => await GenereraArsstatistik(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar årlig personalstatistik i SKR-format.
    /// Innehåller BESTA-kod, AID-kod, sysselsättningsgrad, lön och kön per individ.
    /// </summary>
    private Task<IntegrationResult> GenereraArsstatistik(IntegrationRequest request, CancellationToken ct)
    {
        ArsstatistikInput? input;
        try
        {
            input = request.Payload is ArsstatistikInput a
                ? a
                : JsonSerializer.Deserialize<ArsstatistikInput>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för årsstatistik"));
        }

        if (input is null)
            return Task.FromResult(new IntegrationResult(false, "Årsstatistikinput saknas"));

        if (input.Individer.Count == 0)
            return Task.FromResult(new IntegrationResult(false, "Inga individer att rapportera"));

        var filinnehall = GenereraSKRFil(input);
        var fileName = $"SKR_STAT_{input.Organisationsnummer}_{input.Ar}.txt";

        return Task.FromResult(new IntegrationResult(
            true,
            $"SKR-statistik genererad: {fileName} ({input.Individer.Count} individer)",
            new SKRStatistikFil(fileName, filinnehall)));
    }

    internal string GenereraSKRFil(ArsstatistikInput input)
    {
        var sb = new StringBuilder();

        // Header: Typ(1) | OrgNr(10) | År(4) | Antal(6) | System(10)
        sb.AppendLine(string.Join("|",
            "H",
            input.Organisationsnummer.PadRight(10),
            input.Ar.ToString(),
            input.Individer.Count.ToString().PadLeft(6, '0'),
            "RegionHR".PadRight(10)));

        foreach (var ind in input.Individer)
        {
            // Detaljrad: Typ(1) | Personnummer(12) | BESTA(4) | AID(5) | Sysgrad(5) | Lön(8) | Kön(1)
            sb.AppendLine(string.Join("|",
                "D",
                ind.Personnummer.PadRight(12),
                ind.BESTAKod.PadRight(4),
                ind.AIDKod.PadRight(5),
                ind.Sysselsattningsgrad.ToString("F1", CultureInfo.InvariantCulture).PadLeft(5),
                ind.Manadslon.ToString("F0", CultureInfo.InvariantCulture).PadLeft(8),
                ind.Kon));
        }

        // Footer
        var snittlon = input.Individer.Count > 0
            ? input.Individer.Average(i => i.Manadslon)
            : 0m;
        sb.AppendLine($"T|{input.Individer.Count}|{snittlon.ToString("F0", CultureInfo.InvariantCulture)}");

        return sb.ToString();
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera anslutning till SKR:s portal
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class ArsstatistikInput
{
    public int Ar { get; set; }
    public string Organisationsnummer { get; set; } = string.Empty;
    public List<StatistikIndivid> Individer { get; set; } = [];
}

public sealed class StatistikIndivid
{
    public string Personnummer { get; set; } = string.Empty;
    public string BESTAKod { get; set; } = string.Empty;      // BESTA-klassificering
    public string AIDKod { get; set; } = string.Empty;         // AID-kod (befattning)
    public decimal Sysselsattningsgrad { get; set; }            // 0-100%
    public decimal Manadslon { get; set; }
    public string Kon { get; set; } = string.Empty;             // "M" eller "K"
}

public sealed record SKRStatistikFil(string FileName, string Content);
