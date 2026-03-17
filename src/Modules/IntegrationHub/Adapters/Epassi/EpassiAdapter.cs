using System.Globalization;
using System.Text;
using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Epassi;

/// <summary>
/// Adapter mot Epassi (friskvårdsbidrag).
/// SFTP-baserad export av friskvårdsbidragsdata.
/// Output: CSV-fil med individers friskvårdssaldon.
/// </summary>
public sealed class EpassiAdapter : IIntegrationAdapter
{
    public string SystemName => "Epassi";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraFriskvardsfil" => await GenereraFriskvardsfil(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar CSV-fil med friskvårdsbidragsdata för SFTP-överföring till Epassi.
    /// </summary>
    private Task<IntegrationResult> GenereraFriskvardsfil(IntegrationRequest request, CancellationToken ct)
    {
        FriskvardInput? input;
        try
        {
            input = request.Payload is FriskvardInput f
                ? f
                : JsonSerializer.Deserialize<FriskvardInput>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för friskvårdsfil"));
        }

        if (input is null)
            return Task.FromResult(new IntegrationResult(false, "Friskvårdsinput saknas"));

        if (input.Individer.Count == 0)
            return Task.FromResult(new IntegrationResult(false, "Inga individer att exportera"));

        var csv = GenereraCsv(input);
        var fileName = $"EPASSI_FRISKVARD_{DateTime.UtcNow:yyyyMMdd}.csv";

        return Task.FromResult(new IntegrationResult(
            true,
            $"Friskvårdsfil genererad: {fileName} ({input.Individer.Count} individer)",
            new EpassiFil(fileName, csv)));
    }

    internal static string GenereraCsv(FriskvardInput input)
    {
        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Personnummer;Namn;Bidrag;AnvantBelopp;KvarvarandeSaldo");

        foreach (var ind in input.Individer)
        {
            var kvarvarande = ind.Bidrag - ind.AnvantBelopp;
            sb.AppendLine(string.Join(";",
                ind.Personnummer,
                ind.Namn,
                ind.Bidrag.ToString("F2", CultureInfo.InvariantCulture),
                ind.AnvantBelopp.ToString("F2", CultureInfo.InvariantCulture),
                kvarvarande.ToString("F2", CultureInfo.InvariantCulture)));
        }

        return sb.ToString();
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera SFTP-anslutning till Epassi
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class FriskvardInput
{
    public List<FriskvardIndivid> Individer { get; set; } = [];
}

public sealed class FriskvardIndivid
{
    public string Personnummer { get; set; } = string.Empty;
    public string Namn { get; set; } = string.Empty;
    public decimal Bidrag { get; set; }           // Tilldelat friskvårdsbidrag
    public decimal AnvantBelopp { get; set; }     // Redan använt belopp
}

public sealed record EpassiFil(string FileName, string Content);
