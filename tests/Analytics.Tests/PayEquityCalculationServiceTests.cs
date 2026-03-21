using RegionHR.Analytics.Domain;
using RegionHR.Infrastructure.Services;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class PayEquityCalculationServiceTests
{
    [Fact]
    public void CalculateMedian_WithOddCount_ReturnsMiddleValue()
    {
        var values = new[] { 25000m, 30000m, 35000m };
        var median = PayEquityCalculationService.CalculateMedian(values);
        Assert.Equal(30000m, median);
    }

    [Fact]
    public void CalculateMedian_WithEvenCount_ReturnsAverage()
    {
        var values = new[] { 25000m, 30000m, 35000m, 40000m };
        var median = PayEquityCalculationService.CalculateMedian(values);
        Assert.Equal(32500m, median);
    }

    [Fact]
    public void CalculateMedian_WithSingleValue_ReturnsThatValue()
    {
        var values = new[] { 42000m };
        var median = PayEquityCalculationService.CalculateMedian(values);
        Assert.Equal(42000m, median);
    }

    [Fact]
    public void CalculateMedian_WithEmpty_ReturnsZero()
    {
        var values = Array.Empty<decimal>();
        var median = PayEquityCalculationService.CalculateMedian(values);
        Assert.Equal(0m, median);
    }

    [Fact]
    public void BeraknaAtgardsKostnad_CalculatesCorrectly()
    {
        // MedelLonMan = 35000, MedelLonKvinnor = 33000
        // Gap = (35000 - 33000) / 35000 * 100 = 5.71%
        // To close to 0%: women need 35000 - 33000 = 2000 kr raise
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Sjukskoterska", 10, 5,
            33000m, 35000m, 33000m, 35000m, 5.71m, null, "{}");

        var result = PayEquityCalculationService.BeraknaAtgardsKostnad(analysis);

        Assert.Equal(2000m, result.HojningPerKvinnaManad);
        Assert.Equal(20000m, result.TotalManadskostnad); // 2000 * 10 women
        // 20000 * 12 * 1.3142 = 315408
        Assert.Equal(315408m, result.TotalArskostnadInklAvgifter);
    }

    [Fact]
    public void BeraknaAtgardsKostnad_WithTargetGap_CalculatesPartialClosure()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Lakare", 10, 10,
            58000m, 62000m, 58000m, 62000m, 6.45m, null, "{}");

        // Target 3% gap: women target = 62000 * (1 - 0.03) = 60140
        // Raise per woman = 60140 - 58000 = 2140
        var result = PayEquityCalculationService.BeraknaAtgardsKostnad(analysis, 3m);

        Assert.Equal(2140m, result.HojningPerKvinnaManad);
        Assert.Equal(21400m, result.TotalManadskostnad); // 2140 * 10 women
    }

    [Fact]
    public void BeraknaAtgardsKostnad_WhenNoGap_ReturnsZero()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Test", 10, 10,
            35000m, 35000m, 35000m, 35000m, 0m, null, "{}");

        var result = PayEquityCalculationService.BeraknaAtgardsKostnad(analysis);

        Assert.Equal(0m, result.HojningPerKvinnaManad);
        Assert.Equal(0m, result.TotalManadskostnad);
    }

    [Fact]
    public void BeraknaAtgardsKostnad_WhenNoWomen_ReturnsZero()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Test", 0, 10,
            0m, 35000m, 0m, 35000m, 100m, null, "{}");

        var result = PayEquityCalculationService.BeraknaAtgardsKostnad(analysis);

        Assert.Equal(0m, result.TotalManadskostnad);
    }
}
