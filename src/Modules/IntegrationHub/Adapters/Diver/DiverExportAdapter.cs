using System.Globalization;
using System.Text;
using System.Text.Json;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.Diver;

/// <summary>
/// Adapter mot Diver (analysverktyg).
/// Exporterar HR-mätetal per enhet i CSV-format för analys.
/// </summary>
public sealed class DiverExportAdapter : IIntegrationAdapter
{
    public string SystemName => "Diver";

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraAnalysdata" => await GenereraAnalysdata(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar CSV med HR-mätetal per enhet för Diver-analys.
    /// </summary>
    private Task<IntegrationResult> GenereraAnalysdata(IntegrationRequest request, CancellationToken ct)
    {
        DiverExportInput? input;
        try
        {
            input = request.Payload is DiverExportInput d
                ? d
                : JsonSerializer.Deserialize<DiverExportInput>(request.Payload?.ToString() ?? "");
        }
        catch
        {
            return Task.FromResult(new IntegrationResult(false, "Ogiltig payload för analysdata"));
        }

        if (input is null)
            return Task.FromResult(new IntegrationResult(false, "Analysdatainput saknas"));

        if (input.Enheter.Count == 0)
            return Task.FromResult(new IntegrationResult(false, "Inga enheter att exportera"));

        var csv = GenereraCsv(input);
        var fileName = $"DIVER_HR_{input.Period}.csv";

        return Task.FromResult(new IntegrationResult(
            true,
            $"Diver-analysdata genererad: {fileName} ({input.Enheter.Count} enheter)",
            new DiverExportFil(fileName, csv)));
    }

    internal static string GenereraCsv(DiverExportInput input)
    {
        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Period;EnhetId;EnhetNamn;AntalAnstallda;Sysselsattningsgrad;SjukfranvaroProcent;PersonalomsattningProcent;Lonekostnad;OvertidTimmar");

        foreach (var enhet in input.Enheter)
        {
            sb.AppendLine(string.Join(";",
                input.Period,
                enhet.EnhetId,
                enhet.EnhetNamn,
                enhet.AntalAnstallda.ToString(),
                enhet.Sysselsattningsgrad.ToString("F1", CultureInfo.InvariantCulture),
                enhet.SjukfranvaroProcent.ToString("F1", CultureInfo.InvariantCulture),
                enhet.PersonalomsattningProcent.ToString("F1", CultureInfo.InvariantCulture),
                enhet.Lonekostnad.ToString("F2", CultureInfo.InvariantCulture),
                enhet.OvertidTimmar.ToString("F1", CultureInfo.InvariantCulture)));
        }

        return sb.ToString();
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera SFTP/filshare-anslutning till Diver
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class DiverExportInput
{
    public string Period { get; set; } = string.Empty;     // "YYYYMM"
    public List<EnhetMatetal> Enheter { get; set; } = [];
}

public sealed class EnhetMatetal
{
    public string EnhetId { get; set; } = string.Empty;
    public string EnhetNamn { get; set; } = string.Empty;
    public int AntalAnstallda { get; set; }
    public decimal Sysselsattningsgrad { get; set; }
    public decimal SjukfranvaroProcent { get; set; }
    public decimal PersonalomsattningProcent { get; set; }
    public decimal Lonekostnad { get; set; }
    public decimal OvertidTimmar { get; set; }
}

public sealed record DiverExportFil(string FileName, string Content);
