using Xunit;
using RegionHR.Analytics.Domain;
using RegionHR.Infrastructure.Services;

namespace RegionHR.Analytics.Tests;

public class ScenarioCalculationServiceTests
{
    [Fact]
    public void ProjectForMonth_NoAssumptions_ReturnsBase()
    {
        var (headcount, fte, salary) = ScenarioCalculationService.ProjectForMonth(
            100, 95.5m, 3_000_000m, 6, []);

        Assert.Equal(100, headcount);
        Assert.Equal(95.5m, fte);
        Assert.Equal(3_000_000m, salary);
    }

    [Fact]
    public void ProjectForMonth_HeadcountChange_IncreasesLinearly()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "HeadcountChange", 2m, "Okning med 2 per manad")
        };

        var (headcount, fte, _) = ScenarioCalculationService.ProjectForMonth(
            100, 95m, 3_000_000m, 3, assumptions);

        Assert.Equal(106, headcount); // 100 + 2*3
        Assert.Equal(101m, fte);      // 95 + 2*3
    }

    [Fact]
    public void ProjectForMonth_AttritionRate_DecreasesExponentially()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "AttritionRate", 12m, "12% arlig omsattning")
        };

        var (headcount, _, _) = ScenarioCalculationService.ProjectForMonth(
            100, 95m, 3_000_000m, 12, assumptions);

        // 12% annual attrition => ~88 after 12 months
        Assert.True(headcount >= 86 && headcount <= 90, $"Expected ~88, got {headcount}");
    }

    [Fact]
    public void ProjectForMonth_SalaryIncrease_IncreasesMonthly()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "SalaryIncrease", 3m, "3% arlig loneokning")
        };

        var (_, _, salary) = ScenarioCalculationService.ProjectForMonth(
            100, 95m, 3_000_000m, 12, assumptions);

        // 3% increase should give ~3,090,000 after 12 months
        Assert.True(salary > 3_000_000m && salary < 3_200_000m, $"Expected ~3,090,000, got {salary}");
    }

    [Fact]
    public void ProjectForMonth_NewHires_IncreasesHeadcountAndCost()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "NewHires", 3m, "3 nyanstallningar per manad")
        };

        var (headcount, _, salary) = ScenarioCalculationService.ProjectForMonth(
            100, 95m, 3_000_000m, 6, assumptions);

        Assert.Equal(118, headcount); // 100 + 3*6
        Assert.True(salary > 3_000_000m, "Salary should increase with new hires");
    }

    [Fact]
    public void ProjectForMonth_CombinedAssumptions()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "AttritionRate", 10m, "10% arlig"),
            ScenarioAssumption.Skapa(Guid.NewGuid(), "SalaryIncrease", 3m, "3% arlig"),
            ScenarioAssumption.Skapa(Guid.NewGuid(), "NewHires", 1m, "1 per manad")
        };

        var (headcount, _, _) = ScenarioCalculationService.ProjectForMonth(
            100, 95m, 3_000_000m, 6, assumptions);

        // Attrition reduces, new hires add
        Assert.True(headcount > 80 && headcount < 120, $"Expected 90-110, got {headcount}");
    }

    [Fact]
    public void ProjectForMonth_NeverReturnsNegative()
    {
        var assumptions = new[]
        {
            ScenarioAssumption.Skapa(Guid.NewGuid(), "AttritionRate", 99m, "99% omsattning")
        };

        var (headcount, fte, salary) = ScenarioCalculationService.ProjectForMonth(
            10, 9m, 300_000m, 12, assumptions);

        Assert.True(headcount >= 0);
        Assert.True(fte >= 0);
        Assert.True(salary >= 0);
    }
}
