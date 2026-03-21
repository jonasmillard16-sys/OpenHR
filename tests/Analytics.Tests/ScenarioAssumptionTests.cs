using Xunit;
using RegionHR.Analytics.Domain;

namespace RegionHR.Analytics.Tests;

public class ScenarioAssumptionTests
{
    [Theory]
    [InlineData("HeadcountChange")]
    [InlineData("AttritionRate")]
    [InlineData("SalaryIncrease")]
    [InlineData("NewHires")]
    [InlineData("FreezeHiring")]
    public void Skapa_AcceptsValidTypes(string typ)
    {
        var scenarioId = Guid.NewGuid();
        var assumption = ScenarioAssumption.Skapa(scenarioId, typ, 10m, "Test");

        Assert.NotEqual(Guid.Empty, assumption.Id);
        Assert.Equal(scenarioId, assumption.ScenarioId);
        Assert.Equal(typ, assumption.Typ);
        Assert.Equal(10m, assumption.Värde);
    }

    [Fact]
    public void Skapa_ThrowsForInvalidType()
    {
        Assert.Throws<ArgumentException>(() =>
            ScenarioAssumption.Skapa(Guid.NewGuid(), "InvalidType", 10m, "Test"));
    }

    [Fact]
    public void Skapa_SetsOptionalEnhetId()
    {
        var enhetId = Guid.NewGuid();
        var assumption = ScenarioAssumption.Skapa(Guid.NewGuid(), "AttritionRate", 8m, "Test", enhetId);

        Assert.Equal(enhetId, assumption.EnhetId);
    }
}
