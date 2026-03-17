using System.Text.Json;
using System.Text.Json.Serialization;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Adapters.PowerBI;

/// <summary>
/// Adapter för export av HR analytics-data till Power BI.
/// Genererar JSON-format med nyckeltal för sjukfrånvaro, bemanning och personalomsättning.
/// </summary>
public sealed class PowerBIExportAdapter : IIntegrationAdapter
{
    public string SystemName => "PowerBI";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default)
    {
        return request.OperationType switch
        {
            "GenereraAnalyticsExport" => await GenereraAnalyticsExport(request, ct),
            _ => new IntegrationResult(false, $"Okänd operation: {request.OperationType}")
        };
    }

    /// <summary>
    /// Genererar HR analytics-export i JSON-format för Power BI.
    /// Inkluderar sjukfrånvaro-KPI, bemanningsmätetal och personalomsättningsdata.
    /// </summary>
    private Task<IntegrationResult> GenereraAnalyticsExport(IntegrationRequest request, CancellationToken ct)
    {
        var period = request.Metadata?.GetValueOrDefault("Period") ?? DateTime.UtcNow.ToString("yyyyMM");

        // I produktion: hämta data från respektive modul via contracts
        var export = new PowerBIExport
        {
            Period = period,
            GenereradDatum = DateTime.UtcNow,
            SjukfranvaroKPI = new SjukfranvaroKPI
            {
                KortSjukfranvaroProcent = 2.1m,
                LangSjukfranvaroProcent = 3.4m,
                TotalSjukfranvaroProcent = 5.5m,
                AntalSjukfall = 145,
                AntalRehabArenden = 23
            },
            Bemanningsmatetal = new Bemanningsmatetal
            {
                AntalTillsvidareanstallda = 8500,
                AntalVisstidsanstallda = 420,
                AntalTimanstallda = 310,
                Sysselsattningsgrad = 92.3m,
                BemanningsradKvot = 1.05m
            },
            PersonalomsattningData = new PersonalomsattningData
            {
                NyanstallningarAntal = 85,
                AvslutAntal = 62,
                PersonalomsattningProcent = 7.3m,
                InternMobilitetAntal = 34,
                GenomsnittligAnstallningstidManader = 96
            }
        };

        var json = JsonSerializer.Serialize(export, JsonOptions);

        return Task.FromResult(new IntegrationResult(
            true,
            $"Analytics-export genererad för period {period}",
            new PowerBIExportFil($"POWERBI_HR_{period}.json", json)));
    }

    public Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        // I produktion: verifiera anslutning till Power BI datagateway
        return Task.FromResult(true);
    }
}

// --- Models ---

public sealed class PowerBIExport
{
    public string Period { get; set; } = string.Empty;
    public DateTime GenereradDatum { get; set; }
    public SjukfranvaroKPI SjukfranvaroKPI { get; set; } = new();
    public Bemanningsmatetal Bemanningsmatetal { get; set; } = new();
    public PersonalomsattningData PersonalomsattningData { get; set; } = new();
}

public sealed class SjukfranvaroKPI
{
    public decimal KortSjukfranvaroProcent { get; set; }    // Dag 1-14
    public decimal LangSjukfranvaroProcent { get; set; }    // Dag 15+
    public decimal TotalSjukfranvaroProcent { get; set; }
    public int AntalSjukfall { get; set; }
    public int AntalRehabArenden { get; set; }
}

public sealed class Bemanningsmatetal
{
    public int AntalTillsvidareanstallda { get; set; }
    public int AntalVisstidsanstallda { get; set; }
    public int AntalTimanstallda { get; set; }
    public decimal Sysselsattningsgrad { get; set; }
    public decimal BemanningsradKvot { get; set; }
}

public sealed class PersonalomsattningData
{
    public int NyanstallningarAntal { get; set; }
    public int AvslutAntal { get; set; }
    public decimal PersonalomsattningProcent { get; set; }
    public int InternMobilitetAntal { get; set; }
    public int GenomsnittligAnstallningstidManader { get; set; }
}

public sealed record PowerBIExportFil(string FileName, string Content);
