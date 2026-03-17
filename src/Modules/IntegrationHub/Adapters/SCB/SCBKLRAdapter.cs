using System.Globalization;
using System.Text;
using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.SCB;

/// <summary>
/// Adapter mot Statistiska Centralbyrån (SCB).
/// Genererar konjunkturlönestatistik (KLR), månatligt den 25:e.
/// Format: textfil med fast bredd per SCB-specifikation.
/// </summary>
public sealed class SCBKLRAdapter : IIntegrationAdapter
{
    public string SystemName => "SCB";

    // Fast bredd per fält i SCB KLR-format
    private const int PERSONNUMMER_WIDTH = 12;
    private const int LON_PER_TIMME_WIDTH = 10;
    private const int ARBETADE_TIMMAR_WIDTH = 8;
    private const int ARSARBETARE_WIDTH = 6;

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraKLRRapport" => await GenereraKLRRapport(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar KLR-rapport (konjunkturlönestatistik) i SCB:s fasta breddformat.
    /// Skickas månadsvis senast den 25:e.
    /// </summary>
    private Task<IntegrationResult> GenereraKLRRapport(IntegrationRequest request, CancellationToken ct)
    {
        KLRInput? input;
        try
        {
            input = request.Payload is KLRInput k
                ? k
                : JsonSerializer.Deserialize<KLRInput>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för KLR-rapport"));
        }

        if (input is null)
            return Task.FromResult(new IntegrationResult(false, "KLR-input saknas"));

        if (input.Individer.Count == 0)
            return Task.FromResult(new IntegrationResult(false, "Inga individer att rapportera"));

        var filinnehall = GenereraFastBreddFil(input);
        var fileName = $"KLR_{input.Organisationsnummer}_{input.Period}.txt";

        return Task.FromResult(new IntegrationResult(
            true,
            $"KLR-rapport genererad: {fileName} ({input.Individer.Count} individer)",
            new KLRRapportFil(fileName, filinnehall)));
    }

    internal string GenereraFastBreddFil(KLRInput input)
    {
        var sb = new StringBuilder();

        // Header (fast bredd): Typ(1) Period(6) OrgNr(10) Antal(6) System(10)
        sb.Append('H');
        sb.Append(input.Period.PadRight(6));
        sb.Append(input.Organisationsnummer.PadRight(10));
        sb.Append(input.Individer.Count.ToString().PadLeft(6, '0'));
        sb.Append("RegionHR  ");
        sb.AppendLine();

        foreach (var ind in input.Individer)
        {
            // Detaljrad (fast bredd): Typ(1) Pnr(12) LönPerTimme(10) ArbetadeTimmar(8) Årsarbetare(6)
            sb.Append('D');
            sb.Append(ind.Personnummer.PadRight(PERSONNUMMER_WIDTH));
            sb.Append(ind.GenomsnittslonPerTimme.ToString("F2", CultureInfo.InvariantCulture).PadLeft(LON_PER_TIMME_WIDTH));
            sb.Append(ind.ArbetadeTimmar.ToString("F1", CultureInfo.InvariantCulture).PadLeft(ARBETADE_TIMMAR_WIDTH));
            sb.Append(ind.Arsarbetare.ToString("F3", CultureInfo.InvariantCulture).PadLeft(ARSARBETARE_WIDTH));
            sb.AppendLine();
        }

        // Footer (fast bredd): Typ(1) Antal(6) SummaTimmar(12)
        var summaTimmar = input.Individer.Sum(i => i.ArbetadeTimmar);
        sb.Append('T');
        sb.Append(input.Individer.Count.ToString().PadLeft(6, '0'));
        sb.Append(summaTimmar.ToString("F1", CultureInfo.InvariantCulture).PadLeft(12));
        sb.AppendLine();

        return sb.ToString();
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera SFTP-anslutning till SCB
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class KLRInput
{
    public string Period { get; set; } = string.Empty;          // "YYYYMM"
    public string Organisationsnummer { get; set; } = string.Empty;
    public List<KLRIndivid> Individer { get; set; } = [];
}

public sealed class KLRIndivid
{
    public string Personnummer { get; set; } = string.Empty;
    public decimal GenomsnittslonPerTimme { get; set; }
    public decimal ArbetadeTimmar { get; set; }
    public decimal Arsarbetare { get; set; }                    // Helårsarbetare (0.000-1.000)
}

public sealed record KLRRapportFil(string FileName, string Content);
