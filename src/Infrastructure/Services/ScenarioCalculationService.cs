using Microsoft.EntityFrameworkCore;
using RegionHR.Analytics.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Beräkningstjänst för workforce planning-scenarier.
/// Tar nuvarande headcount/FTE/lönekostnad från databasen,
/// applicerar scenarioantaganden och projicerar 3/6/12 månader framåt.
/// AG-avgifter: 31.42% (lagstadgad arbetsgivaravgift).
/// </summary>
public class ScenarioCalculationService
{
    private readonly RegionHRDbContext _db;
    private const decimal AGAvgiftSats = 0.3142m;

    public ScenarioCalculationService(RegionHRDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Beräkna ett scenario: hämta basdata, applicera antaganden, skapa resultat för 3/6/12 månader.
    /// </summary>
    public async Task<List<ScenarioResult>> BeräknaAsync(Guid scenarioId, CancellationToken ct = default)
    {
        var scenario = await _db.PlanningScenarios
            .Include(s => s.Antaganden)
            .FirstOrDefaultAsync(s => s.Id == scenarioId, ct);

        if (scenario is null)
            throw new InvalidOperationException($"Scenario {scenarioId} hittades inte.");

        // Hämta nuvarande basdata
        var today = DateOnly.FromDateTime(DateTime.Today);
        var activeEmployments = await _db.Employments
            .AsNoTracking()
            .Where(e => e.Giltighetsperiod.End == null || e.Giltighetsperiod.End > today)
            .ToListAsync(ct);

        var currentHeadcount = activeEmployments.Count;
        var currentFTE = activeEmployments.Count > 0
            ? activeEmployments.Sum(e => (decimal)e.Sysselsattningsgrad) / 100m
            : 0m;
        var currentSalaryCost = activeEmployments.Count > 0
            ? activeEmployments.Sum(e => e.Manadslon.Amount)
            : 0m;

        // Hämta budget för basåret
        var budgets = await _db.HeadcountPlans
            .AsNoTracking()
            .Where(h => h.Ar == scenario.BasÅr)
            .ToListAsync(ct);
        var totalBudget = budgets.Sum(b => b.BudgeteradKostnad);

        // Ta bort gamla resultat för detta scenario
        var oldResults = await _db.ScenarioResults
            .Where(r => r.ScenarioId == scenarioId)
            .ToListAsync(ct);
        _db.ScenarioResults.RemoveRange(oldResults);

        // Beräkna per period (1-12 månader)
        var results = new List<ScenarioResult>();
        var assumptions = scenario.Antaganden;

        for (int month = 1; month <= 12; month++)
        {
            var periodDate = today.AddMonths(month);
            var period = $"{periodDate.Year}-{periodDate.Month:D2}";

            var (projectedHeadcount, projectedFTE, projectedSalaryCost) =
                ProjectForMonth(currentHeadcount, currentFTE, currentSalaryCost, month, assumptions);

            var agAvgifter = Math.Round(projectedSalaryCost * AGAvgiftSats, 2);
            var totalKostnad = projectedSalaryCost + agAvgifter;
            var deltaMotBudget = totalBudget > 0 ? totalKostnad - (totalBudget / 12m) : 0m;

            var result = ScenarioResult.Skapa(
                scenarioId, period,
                projectedHeadcount, projectedFTE,
                projectedSalaryCost, agAvgifter,
                totalKostnad, Math.Round(deltaMotBudget, 2));

            results.Add(result);
            _db.ScenarioResults.Add(result);
        }

        await _db.SaveChangesAsync(ct);
        return results;
    }

    /// <summary>
    /// Projicera headcount, FTE och lönekostnad framåt en viss antal månader
    /// baserat på antagandena.
    /// </summary>
    internal static (int Headcount, decimal FTE, decimal SalaryCost) ProjectForMonth(
        int baseHeadcount,
        decimal baseFTE,
        decimal baseSalaryCost,
        int monthOffset,
        IEnumerable<ScenarioAssumption> assumptions)
    {
        var headcount = (decimal)baseHeadcount;
        var fte = baseFTE;
        var salaryCost = baseSalaryCost;

        foreach (var assumption in assumptions)
        {
            switch (assumption.Typ)
            {
                case "HeadcountChange":
                    // Värde = antal nya/borttagna per månad
                    headcount += assumption.Värde * monthOffset;
                    fte += assumption.Värde * monthOffset;
                    break;

                case "AttritionRate":
                    // Värde = årlig attrition i procent (t.ex. 10 = 10%)
                    var monthlyAttrition = assumption.Värde / 100m / 12m;
                    var attritionFactor = (decimal)Math.Pow((double)(1m - monthlyAttrition), monthOffset);
                    headcount *= attritionFactor;
                    fte *= attritionFactor;
                    salaryCost *= attritionFactor;
                    break;

                case "SalaryIncrease":
                    // Värde = årlig löneökning i procent (t.ex. 3 = 3%)
                    var monthlySalaryIncrease = assumption.Värde / 100m / 12m;
                    salaryCost *= 1m + (monthlySalaryIncrease * monthOffset);
                    break;

                case "NewHires":
                    // Värde = antal nyanställningar per månad
                    headcount += assumption.Värde * monthOffset;
                    fte += assumption.Värde * monthOffset;
                    // Uppskattad genomsnittlig lönekostnad per person
                    var avgSalaryPerPerson = baseHeadcount > 0 ? baseSalaryCost / baseHeadcount : 35000m;
                    salaryCost += assumption.Värde * monthOffset * avgSalaryPerPerson;
                    break;

                case "FreezeHiring":
                    // Värde = 1 för fryst (ingen nyanställning kompenserar attrition)
                    // Redan hanterat genom att inte lägga till NewHires
                    break;
            }
        }

        return (
            Math.Max(0, (int)Math.Round(headcount)),
            Math.Max(0m, Math.Round(fte, 2)),
            Math.Max(0m, Math.Round(salaryCost, 2))
        );
    }
}
