using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationRuleTests
{
    [Fact]
    public void Skapa_SkaparAktivRegel()
    {
        var kategoriId = AutomationCategoryId.New();
        var regel = AutomationRule.Skapa(
            "LAS-varning", kategoriId, "EmploymentCreated",
            "{\"dagar\": 300}", "{\"typ\": \"notify\"}", AutomationLevel.Notify);

        Assert.True(regel.ArAktiv);
        Assert.Equal("LAS-varning", regel.Namn);
        Assert.Equal(kategoriId, regel.KategoriId);
        Assert.Equal(AutomationLevel.Notify, regel.MinimumNiva);
        Assert.False(regel.ArSystemRegel);
    }

    [Fact]
    public void Inaktivera_VanligRegel_Lyckas()
    {
        var regel = AutomationRule.Skapa(
            "Testregel", AutomationCategoryId.New(), "TestTrigger",
            "{}", "{}", AutomationLevel.Notify);

        regel.Inaktivera();

        Assert.False(regel.ArAktiv);
    }

    [Fact]
    public void Inaktivera_SystemRegel_KastarException()
    {
        var regel = AutomationRule.Skapa(
            "ATL-block", AutomationCategoryId.New(), "ShiftCreated",
            "{}", "{}", AutomationLevel.Block, arSystemRegel: true);

        Assert.Throws<InvalidOperationException>(() => regel.Inaktivera());
    }

    [Fact]
    public void Aktivera_EfterInaktivering_Lyckas()
    {
        var regel = AutomationRule.Skapa(
            "Testregel", AutomationCategoryId.New(), "TestTrigger",
            "{}", "{}", AutomationLevel.Notify);

        regel.Inaktivera();
        regel.Aktivera();

        Assert.True(regel.ArAktiv);
    }

    [Fact]
    public void Skapa_UtanNamn_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            AutomationRule.Skapa("", AutomationCategoryId.New(), "TestTrigger",
                "{}", "{}", AutomationLevel.Notify));
    }

    [Fact]
    public void Skapa_UtanTriggerTyp_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            AutomationRule.Skapa("Testregel", AutomationCategoryId.New(), "",
                "{}", "{}", AutomationLevel.Notify));
    }
}
