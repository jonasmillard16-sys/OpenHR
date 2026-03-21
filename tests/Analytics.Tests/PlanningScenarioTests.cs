using Xunit;
using RegionHR.Analytics.Domain;

namespace RegionHR.Analytics.Tests;

public class PlanningScenarioTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var scenario = PlanningScenario.Skapa("Basscenario", "Testbeskrivning", 2026, "Admin");

        Assert.NotEqual(Guid.Empty, scenario.Id);
        Assert.Equal("Basscenario", scenario.Namn);
        Assert.Equal("Testbeskrivning", scenario.Beskrivning);
        Assert.Equal(2026, scenario.BasÅr);
        Assert.Equal("Draft", scenario.Status);
        Assert.Equal("Admin", scenario.SkapadAv);
    }

    [Fact]
    public void Aktivera_ChangeStatusToActive()
    {
        var scenario = PlanningScenario.Skapa("Test", "Desc", 2026, "Admin");

        scenario.Aktivera();

        Assert.Equal("Active", scenario.Status);
    }

    [Fact]
    public void Aktivera_ThrowsIfAlreadyActive()
    {
        var scenario = PlanningScenario.Skapa("Test", "Desc", 2026, "Admin");
        scenario.Aktivera();

        Assert.Throws<InvalidOperationException>(() => scenario.Aktivera());
    }

    [Fact]
    public void Arkivera_ChangeStatusToArchived()
    {
        var scenario = PlanningScenario.Skapa("Test", "Desc", 2026, "Admin");

        scenario.Arkivera();

        Assert.Equal("Archived", scenario.Status);
    }

    [Fact]
    public void Arkivera_ThrowsIfAlreadyArchived()
    {
        var scenario = PlanningScenario.Skapa("Test", "Desc", 2026, "Admin");
        scenario.Arkivera();

        Assert.Throws<InvalidOperationException>(() => scenario.Arkivera());
    }
}
