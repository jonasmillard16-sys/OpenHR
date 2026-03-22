using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Scheduling;

/// <summary>
/// Generates demand forecasts from 12 weeks of historical shift data.
/// Calculates average staffing per weekday per shift type (Dag/Kvall/Natt)
/// and applies seasonal factors from DemandPattern if available.
/// </summary>
public class DemandForecastGenerator
{
    private readonly RegionHRDbContext _db;
    private readonly ILogger<DemandForecastGenerator> _logger;

    public DemandForecastGenerator(
        RegionHRDbContext db,
        ILogger<DemandForecastGenerator> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Generates demand forecasts for the next 4 weeks for all organizational units
    /// that have historical shift data.
    /// </summary>
    public async Task GenerateAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var twelveWeeksAgo = today.AddDays(-84); // 12 weeks

        _logger.LogInformation(
            "DemandForecastGenerator: Genererar efterfrågeprognos från {From} till {To}",
            twelveWeeksAgo, today);

        // Load all shifts from the last 12 weeks
        var historicalShifts = await _db.ScheduledShifts
            .AsNoTracking()
            .Where(s => s.Datum >= twelveWeeksAgo && s.Datum <= today)
            .ToListAsync(ct);

        if (historicalShifts.Count == 0)
        {
            _logger.LogWarning("DemandForecastGenerator: Inga historiska pass hittades");
            return;
        }

        // Load all demand patterns for seasonal adjustment
        var demandPatterns = await _db.DemandPatterns
            .AsNoTracking()
            .ToListAsync(ct);

        // Group by unit (via schedule) — we use schedule ID as proxy for unit
        // since ScheduledShift doesn't have EnhetId directly
        // Group by weekday + shift type across all units
        var shiftsByWeekdayAndType = historicalShifts
            .GroupBy(s => new { Veckodag = (int)s.Datum.DayOfWeek, PassTyp = s.PassTyp })
            .ToDictionary(
                g => g.Key,
                g => g.ToList());

        // Get distinct organizational units from employment data for forecasting
        var orgUnits = await _db.Employments
            .AsNoTracking()
            .Select(e => e.EnhetId)
            .Distinct()
            .ToListAsync(ct);

        if (orgUnits.Count == 0)
        {
            // Fallback: use a default org unit
            orgUnits = [new OrganizationId(Guid.Empty)];
        }

        // Remove existing forecasts for the next 4 weeks to avoid duplicates
        var forecastStart = today.AddDays(1);
        var forecastEnd = today.AddDays(28);

        var existingForecasts = await _db.DemandForecasts
            .Where(f => f.Datum >= forecastStart && f.Datum <= forecastEnd)
            .ToListAsync(ct);

        _db.DemandForecasts.RemoveRange(existingForecasts);

        // Generate forecasts for next 4 weeks (28 days) per org unit
        var createdCount = 0;
        foreach (var enhetId in orgUnits)
        {
            for (var date = forecastStart; date <= forecastEnd; date = date.AddDays(1))
            {
                var veckodag = (int)date.DayOfWeek; // 0=Sunday..6=Saturday

                // Determine relevant shift types for the day
                var shiftTypes = new[] { ShiftType.Dag, ShiftType.Kvall, ShiftType.Natt };

                foreach (var shiftType in shiftTypes)
                {
                    var key = new { Veckodag = veckodag, PassTyp = shiftType };

                    if (!shiftsByWeekdayAndType.TryGetValue(key, out var matchingShifts)
                        || matchingShifts.Count == 0)
                        continue;

                    // Count distinct weeks that had data
                    var weeksWithData = matchingShifts
                        .Select(s => ISOWeek(s.Datum))
                        .Distinct()
                        .Count();

                    if (weeksWithData == 0) continue;

                    // Average staff per occurrence
                    var avgStaffCount = (decimal)matchingShifts.Count / weeksWithData;

                    // Average hours per shift
                    var avgTimmar = matchingShifts.Count > 0
                        ? matchingShifts.Average(s => (double)s.PlaneradeTimmar)
                        : 8.0;

                    // Apply seasonal factor from DemandPattern if available
                    var seasonalFactor = GetSeasonalFactor(demandPatterns, enhetId, veckodag, date);
                    var adjustedStaff = Math.Max(1, (int)Math.Round(avgStaffCount * seasonalFactor));
                    var adjustedTimmar = (decimal)(avgTimmar * (double)seasonalFactor);

                    // Confidence based on weeks of data
                    var konfidensgrad = Math.Min(95m, weeksWithData * 8m); // max 95%

                    var forecast = DemandForecast.Skapa(
                        enhetId,
                        date,
                        adjustedStaff,
                        adjustedTimmar,
                        konfidensgrad);

                    _db.DemandForecasts.Add(forecast);
                    createdCount++;
                }
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DemandForecastGenerator: Skapade {Count} efterfrågeprognoser för {Days} dagar framåt",
            createdCount, 28);
    }

    /// <summary>
    /// Returns the seasonal variation factor for the given unit, weekday and date.
    /// Falls back to 1.0 if no pattern exists.
    /// </summary>
    private static decimal GetSeasonalFactor(
        IReadOnlyList<DemandPattern> patterns,
        OrganizationId enhetId,
        int veckodag,
        DateOnly datum)
    {
        // Try to find a unit-specific pattern first
        var pattern = patterns.FirstOrDefault(p =>
            p.EnhetId == enhetId && p.Veckodag == veckodag);

        // Fall back to any pattern for this weekday
        pattern ??= patterns.FirstOrDefault(p => p.Veckodag == veckodag);

        return pattern?.SasongsVariation ?? 1.0m;
    }

    /// <summary>Returns an approximate ISO week number for the given date.</summary>
    private static int ISOWeek(DateOnly date)
    {
        return System.Globalization.ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));
    }
}
