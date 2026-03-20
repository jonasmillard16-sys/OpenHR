using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationSuggestionTests
{
    [Fact]
    public void Skapa_SkaparVantandeForslag()
    {
        var regelId = AutomationRuleId.New();
        var forslag = AutomationSuggestion.Skapa(regelId, "Skicka LAS-varning");

        Assert.Equal(SuggestionStatus.Pending, forslag.Status);
        Assert.Equal("Skicka LAS-varning", forslag.ForeslagenAtgard);
        Assert.Equal(regelId, forslag.RegelId);
        Assert.Null(forslag.SkapadFor);
    }

    [Fact]
    public void Skapa_MedAnstallId_SpararsKorrekt()
    {
        var empId = EmployeeId.New();
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd", empId);

        Assert.Equal(empId, forslag.SkapadFor);
    }

    [Fact]
    public void Acceptera_VantandeForslag_Lyckas()
    {
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd");

        forslag.Acceptera();

        Assert.Equal(SuggestionStatus.Accepted, forslag.Status);
    }

    [Fact]
    public void Avvisa_VantandeForslag_Lyckas()
    {
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd");

        forslag.Avvisa();

        Assert.Equal(SuggestionStatus.Dismissed, forslag.Status);
    }

    [Fact]
    public void Acceptera_RedanAccepterat_KastarException()
    {
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd");
        forslag.Acceptera();

        Assert.Throws<InvalidOperationException>(() => forslag.Acceptera());
    }

    [Fact]
    public void Avvisa_RedanAvvisat_KastarException()
    {
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd");
        forslag.Avvisa();

        Assert.Throws<InvalidOperationException>(() => forslag.Avvisa());
    }

    [Fact]
    public void GiltigTill_ArFramtiden()
    {
        var forslag = AutomationSuggestion.Skapa(AutomationRuleId.New(), "Åtgärd", giltigDagar: 14);

        Assert.True(forslag.GiltigTill > DateTime.UtcNow);
    }
}
