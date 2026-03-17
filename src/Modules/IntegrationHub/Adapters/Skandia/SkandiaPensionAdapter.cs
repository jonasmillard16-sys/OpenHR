using System.Globalization;
using System.Text;
using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Skandia;

/// <summary>
/// Adapter mot Skandia för pensionsrapportering.
/// Genererar månadsvis pensionsrapport med AKAP-KR premier.
/// AKAP-KR: 6% under 7.5 IBB, 31.5% över 7.5 IBB.
/// Filformat: Skandia-specifikt pipe-separated format.
/// </summary>
public sealed class SkandiaPensionAdapter : IIntegrationAdapter
{
    public string SystemName => "Skandia";

    /// <summary>
    /// Inkomstbasbelopp 2026 (uppskattat). Uppdateras årligen.
    /// </summary>
    private const decimal IBB_2026 = 80600m;
    private const decimal GRANS_7_5_IBB = IBB_2026 * 7.5m;

    /// <summary>AKAP-KR avgift under 7.5 IBB (6%)</summary>
    private const decimal AVGIFT_UNDER_GRANS = 0.06m;

    /// <summary>AKAP-KR avgift över 7.5 IBB (31.5%)</summary>
    private const decimal AVGIFT_OVER_GRANS = 0.315m;

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraPensionsrapport" => await GenereraPensionsrapport(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar månadsvis pensionsrapport i Skandia-specifikt pipe-separated format.
    /// Beräknar AKAP-KR premier: 6% under 7.5 IBB, 31.5% över 7.5 IBB.
    /// </summary>
    private Task<IntegrationResult> GenereraPensionsrapport(IntegrationRequest request, CancellationToken ct)
    {
        PensionsrapportInput? input;
        try
        {
            input = request.Payload is PensionsrapportInput p
                ? p
                : JsonSerializer.Deserialize<PensionsrapportInput>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för pensionsrapport"));
        }

        if (input is null)
            return Task.FromResult(new IntegrationResult(false, "Pensionsrapportinput saknas"));

        if (input.Individer.Count == 0)
            return Task.FromResult(new IntegrationResult(false, "Inga individer att rapportera"));

        var rapport = GenereraPipeSeparatedRapport(input);
        var fileName = $"PENSION_SKANDIA_{input.Period}.txt";

        return Task.FromResult(new IntegrationResult(
            true,
            $"Pensionsrapport genererad: {fileName} ({input.Individer.Count} individer)",
            new SkandiaPensionsrapport(fileName, rapport)));
    }

    internal string GenereraPipeSeparatedRapport(PensionsrapportInput input)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"H|AKAP-KR|{input.Period}|{input.Individer.Count}|RegionHR");

        foreach (var individ in input.Individer)
        {
            // Beräkna avgifter baserat på pensionsgrundande lön
            var (underGrans, overGrans) = BeraknaAKAPKRPremier(individ.PensionsgrundandeLon);

            sb.AppendLine(string.Join("|",
                "D",
                individ.Personnummer,
                individ.PensionsgrundandeLon.ToString("F2", CultureInfo.InvariantCulture),
                underGrans.ToString("F2", CultureInfo.InvariantCulture),
                overGrans.ToString("F2", CultureInfo.InvariantCulture),
                (underGrans + overGrans).ToString("F2", CultureInfo.InvariantCulture)));
        }

        // Footer med summor
        var totalUnder = input.Individer.Sum(i => BeraknaAKAPKRPremier(i.PensionsgrundandeLon).UnderGrans);
        var totalOver = input.Individer.Sum(i => BeraknaAKAPKRPremier(i.PensionsgrundandeLon).OverGrans);

        sb.AppendLine($"T|{input.Individer.Count}|{totalUnder.ToString("F2", CultureInfo.InvariantCulture)}|{totalOver.ToString("F2", CultureInfo.InvariantCulture)}|{(totalUnder + totalOver).ToString("F2", CultureInfo.InvariantCulture)}");

        return sb.ToString();
    }

    /// <summary>
    /// Beräknar AKAP-KR pensionspremier.
    /// Under 7.5 IBB: 6% avgift.
    /// Över 7.5 IBB: 31.5% avgift.
    /// </summary>
    internal static (decimal UnderGrans, decimal OverGrans) BeraknaAKAPKRPremier(decimal pensionsgrundandeLon)
    {
        // Månadsvis gräns = 7.5 IBB / 12
        var manadsGrans = GRANS_7_5_IBB / 12m;

        if (pensionsgrundandeLon <= manadsGrans)
        {
            return (pensionsgrundandeLon * AVGIFT_UNDER_GRANS, 0m);
        }

        var underGrans = manadsGrans * AVGIFT_UNDER_GRANS;
        var overGrans = (pensionsgrundandeLon - manadsGrans) * AVGIFT_OVER_GRANS;
        return (underGrans, overGrans);
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera SFTP-anslutning till Skandia
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class PensionsrapportInput
{
    public string Period { get; set; } = string.Empty;  // "YYYYMM"
    public List<PensionsIndivid> Individer { get; set; } = [];
}

public sealed class PensionsIndivid
{
    public string Personnummer { get; set; } = string.Empty;
    public decimal PensionsgrundandeLon { get; set; }
    public decimal AvgiftUnderGrans { get; set; }
    public decimal AvgiftOverGrans { get; set; }
}

public sealed record SkandiaPensionsrapport(string FileName, string Content);
