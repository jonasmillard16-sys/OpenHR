using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Analytics.Domain;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.Analytics;

/// <summary>
/// Heuristic-based prediction calculation service.
/// Computes attrition risk, sick leave forecast, headcount forecast,
/// and labor cost forecast using rule-based scoring — no ML required.
/// </summary>
public class PredictionCalculationService
{
    private readonly RegionHRDbContext _db;
    private readonly ILogger<PredictionCalculationService> _logger;

    // Known bristyrken (shortage occupations) in Swedish healthcare
    private static readonly string[] Bristyrken =
        ["sjuksköterska", "sjukskoterska", "läkare", "lakare", "barnmorska", "fysioterapeut"];

    public PredictionCalculationService(
        RegionHRDbContext db,
        ILogger<PredictionCalculationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Runs all prediction models and persists PredictionResult records.
    /// </summary>
    public async Task BeraknaAllaAsync(CancellationToken ct = default)
    {
        await BeraknaAttritionRiskAsync(ct);
        await BerknaSickLeaveForecastAsync(ct);
        await BeraknaHeadcountForecastAsync(ct);
        await BeraknaLaborCostForecastAsync(ct);
        await _db.SaveChangesAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Attrition Risk
    // score = (tenure < 2yr ? 20 : 0) + (sick days > 15 ? 15 : 0)
    //       + (visstid ? 20 : 0) + (bristyrke ? 15 : 0)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task BeraknaAttritionRiskAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("PredictionCalculationService: Beräknar attritionsrisk");

        var model = await GetOrCreateModelAsync("Attritionsrisk", "Attrition", ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var twelveMonthsAgo = today.AddMonths(-12);

        var employments = await _db.Employments
            .AsNoTracking()
            .Where(e => e.Giltighetsperiod.Start <= today &&
                        (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= today))
            .ToListAsync(ct);

        // Count sick days per employee over last 12 months
        var sickDaysByEmployee = await _db.SickLeaveNotifications
            .AsNoTracking()
            .Where(s => s.StartDatum >= twelveMonthsAgo)
            .GroupBy(s => s.AnstallId)
            .Select(g => new { AnstallId = g.Key, TotalSickDays = g.Sum(s => s.SjukDag) })
            .ToDictionaryAsync(x => x.AnstallId, x => x.TotalSickDays, ct);

        foreach (var emp in employments)
        {
            var score = 0;
            var faktorer = new List<string>();

            // Tenure < 2 years
            var tenureYears = (today.DayNumber - emp.Giltighetsperiod.Start.DayNumber) / 365.25;
            if (tenureYears < 2.0)
            {
                score += 20;
                faktorer.Add($"Kort anställningstid ({tenureYears:F1} år < 2 år)");
            }

            // Sick days > 15 in last 12 months
            sickDaysByEmployee.TryGetValue(emp.AnstallId.Value, out var sickDays);
            if (sickDays > 15)
            {
                score += 15;
                faktorer.Add($"Hög sjukfrånvaro ({sickDays} dagar senaste 12 månader)");
            }

            // Visstid (tidsbegränsad anställning)
            if (emp.ArTidsbegransad)
            {
                score += 20;
                faktorer.Add($"Tidsbegränsad anställning ({emp.Anstallningsform})");
            }

            // Bristyrke
            if (ArBristyrke(emp.Befattningstitel))
            {
                score += 15;
                faktorer.Add($"Bristyrke ({emp.Befattningstitel})");
            }

            var riskNiva = score switch
            {
                >= 50 => "Critical",
                >= 30 => "High",
                >= 15 => "Medium",
                _ => "Low"
            };

            var result = PredictionResult.Skapa(
                model.Id,
                "Employee",
                emp.AnstallId.Value,
                score,
                riskNiva,
                JsonSerializer.Serialize(faktorer));

            _db.PredictionResults.Add(result);
        }

        model.UppdateraTranning(0.70m); // Estimated heuristic accuracy
        _logger.LogInformation(
            "PredictionCalculationService: Attritionsrisk beräknad för {Count} anställningar",
            employments.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Sick Leave Forecast
    // Average monthly sick days over last 12 months, project next 3 months
    // ─────────────────────────────────────────────────────────────────────────
    public async Task BerknaSickLeaveForecastAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("PredictionCalculationService: Beräknar sjukfrånvaroprognos");

        var model = await GetOrCreateModelAsync("Sjukfrånvaroprognos", "SickLeaveForecast", ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var twelveMonthsAgo = today.AddMonths(-12);

        // Monthly sick day counts over last 12 months
        var sickLeaves = await _db.SickLeaveNotifications
            .AsNoTracking()
            .Where(s => s.StartDatum >= twelveMonthsAgo && s.StartDatum <= today)
            .ToListAsync(ct);

        // Bucket by month
        var monthlyTotals = new Dictionary<int, int>(); // month offset -> sick days
        foreach (var s in sickLeaves)
        {
            var monthOffset = (today.Year - s.StartDatum.Year) * 12
                            + (today.Month - s.StartDatum.Month);
            if (monthOffset < 0 || monthOffset >= 12) continue;
            monthlyTotals.TryGetValue(monthOffset, out var existing);
            monthlyTotals[monthOffset] = existing + s.SjukDag;
        }

        var avgMonthly = monthlyTotals.Count > 0
            ? (decimal)monthlyTotals.Values.Sum() / monthlyTotals.Count
            : 0m;

        // Project next 3 months
        var faktorer = new List<string>
        {
            $"Genomsnittlig sjukfrånvaro senaste 12 månader: {avgMonthly:F1} dagar/månad",
            $"Historiska månader med data: {monthlyTotals.Count}"
        };

        for (int i = 1; i <= 3; i++)
        {
            faktorer.Add($"Prognos månad +{i}: {avgMonthly:F1} dagar");
        }

        var result = PredictionResult.Skapa(
            model.Id,
            "OrgUnit",
            Guid.Empty, // Org-wide forecast
            avgMonthly,
            avgMonthly > 200 ? "High" : avgMonthly > 100 ? "Medium" : "Low",
            JsonSerializer.Serialize(faktorer));

        _db.PredictionResults.Add(result);
        model.UppdateraTranning(0.65m);

        _logger.LogInformation(
            "PredictionCalculationService: Sjukfrånvaroprognos beräknad, genomsnitt {Avg:F1} dagar/månad",
            avgMonthly);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Headcount Forecast
    // Linear extrapolation from last 12 months headcount changes
    // ─────────────────────────────────────────────────────────────────────────
    public async Task BeraknaHeadcountForecastAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("PredictionCalculationService: Beräknar personaltalsrognos");

        var model = await GetOrCreateModelAsync("Personaltalsprognos", "HeadcountForecast", ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Count active employments by start month for last 12 months
        var monthlyStarts = new int[12];
        var allEmployments = await _db.Employments
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var e in allEmployments)
        {
            var monthOffset = (today.Year - e.Giltighetsperiod.Start.Year) * 12
                            + (today.Month - e.Giltighetsperiod.Start.Month);
            if (monthOffset >= 0 && monthOffset < 12)
                monthlyStarts[monthOffset]++;
        }

        var currentHeadcount = allEmployments.Count(e =>
            e.Giltighetsperiod.Start <= today &&
            (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= today));

        // Average monthly change (linear trend)
        var totalStarts12M = monthlyStarts.Sum();
        var avgMonthlyChange = totalStarts12M / 12.0m;

        var faktorer = new List<string>
        {
            $"Nuvarande personalstyrka: {currentHeadcount}",
            $"Genomsnittlig månadsförändring (12 mån): {avgMonthlyChange:F1}",
            $"Prognos +3 månader: {currentHeadcount + (int)(avgMonthlyChange * 3)}",
            $"Prognos +6 månader: {currentHeadcount + (int)(avgMonthlyChange * 6)}",
            $"Prognos +12 månader: {currentHeadcount + (int)(avgMonthlyChange * 12)}"
        };

        var result = PredictionResult.Skapa(
            model.Id,
            "OrgUnit",
            Guid.Empty,
            currentHeadcount,
            "Low",
            JsonSerializer.Serialize(faktorer));

        _db.PredictionResults.Add(result);
        model.UppdateraTranning(0.60m);

        _logger.LogInformation(
            "PredictionCalculationService: Personaltalsprognos beräknad, nuvarande styrka {Count}",
            currentHeadcount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Labor Cost Forecast
    // current total * (1 + avg salary increase rate)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task BeraknaLaborCostForecastAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("PredictionCalculationService: Beräknar lönekostnadsprognos");

        var model = await GetOrCreateModelAsync("Lönekostnadsprognos", "LaborCostForecast", ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeEmployments = await _db.Employments
            .AsNoTracking()
            .Where(e => e.Giltighetsperiod.Start <= today &&
                        (e.Giltighetsperiod.End == null || e.Giltighetsperiod.End >= today))
            .ToListAsync(ct);

        var totalMonthlyWage = activeEmployments.Sum(e => e.Manadslon.Amount);

        // Swedish public sector avg salary increase ~2.5% per year (Allmänna råd)
        const decimal avgSalaryIncreaseRate = 0.025m;
        const decimal employerContributionRate = 0.3142m; // Arbetsgivaravgift

        var totalWithContributions = totalMonthlyWage * (1 + employerContributionRate);
        var annualCost = totalWithContributions * 12;
        var forecastNextYear = annualCost * (1 + avgSalaryIncreaseRate);

        var faktorer = new List<string>
        {
            $"Total månadslön: {totalMonthlyWage:C0}",
            $"Med arbetsgivaravgift (31.42%): {totalWithContributions:C0}/månad",
            $"Nuvarande årskostnad: {annualCost:C0}",
            $"Prognos nästa år (+{avgSalaryIncreaseRate:P0} löneökning): {forecastNextYear:C0}",
            $"Antal aktiva anställda: {activeEmployments.Count}"
        };

        var result = PredictionResult.Skapa(
            model.Id,
            "OrgUnit",
            Guid.Empty,
            annualCost,
            "Low",
            JsonSerializer.Serialize(faktorer));

        _db.PredictionResults.Add(result);
        model.UppdateraTranning(0.80m);

        _logger.LogInformation(
            "PredictionCalculationService: Lönekostnadsprognos beräknad, total årskostnad {Cost:C0}",
            annualCost);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<PredictionModel> GetOrCreateModelAsync(
        string namn, string typ, CancellationToken ct)
    {
        var model = await _db.PredictionModels
            .FirstOrDefaultAsync(m => m.Typ == typ, ct);

        if (model is null)
        {
            model = PredictionModel.Skapa(namn, typ);
            _db.PredictionModels.Add(model);
            await _db.SaveChangesAsync(ct);
        }

        return model;
    }

    private static bool ArBristyrke(string? befattning)
    {
        if (string.IsNullOrWhiteSpace(befattning))
            return false;

        var lower = befattning.ToLowerInvariant();
        return Bristyrken.Any(b => lower.Contains(b));
    }
}
