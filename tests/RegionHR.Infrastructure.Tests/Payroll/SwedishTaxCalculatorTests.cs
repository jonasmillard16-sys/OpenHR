using RegionHR.Infrastructure.Payroll;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Payroll;

public class SwedishTaxCalculatorTests
{
    private readonly SwedishTaxCalculator _calc = new();

    [Fact]
    public void Berakna_NormalLon_CorrectKommunalSkatt()
    {
        var result = _calc.Berakna(32000m);
        Assert.True(result.KommunalSkatt > 0);
        Assert.True(result.KommunalSkatt < result.Brutto);
    }

    [Fact]
    public void Berakna_NormalLon_NoStatligSkatt()
    {
        var result = _calc.Berakna(32000m);
        Assert.Equal(0m, result.StatligSkatt);
    }

    [Fact]
    public void Berakna_HogLon_HasStatligSkatt()
    {
        var result = _calc.Berakna(60000m);
        Assert.True(result.StatligSkatt > 0);
    }

    [Fact]
    public void Berakna_Arbetsgivaravgift_Correct()
    {
        var result = _calc.Berakna(32000m);
        Assert.Equal(Math.Round(32000m * 0.3142m, 0), result.Arbetsgivaravgift);
    }

    [Fact]
    public void Berakna_NettoLessThanBrutto()
    {
        var result = _calc.Berakna(32000m);
        Assert.True(result.Netto < result.Brutto);
        Assert.True(result.Netto > 0);
    }

    [Fact]
    public void Berakna_TotalKostnad_GreaterThanBrutto()
    {
        var result = _calc.Berakna(32000m);
        Assert.True(result.TotalKostnad > result.Brutto);
    }
}
