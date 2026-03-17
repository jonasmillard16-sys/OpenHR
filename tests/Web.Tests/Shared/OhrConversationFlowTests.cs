using Bunit;
using RegionHR.Web.Components.Shared;
using Xunit;

namespace RegionHR.Web.Tests.Shared;

public class OhrConversationFlowTests : BunitContext
{
    [Fact]
    public void Shows_current_step_label()
    {
        var cut = Render<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Välj dag", "Bekräfta", "Klart" })
            .Add(c => c.CurrentStep, 0)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Innehåll")));

        Assert.Contains("Välj dag", cut.Markup);
    }

    [Fact]
    public void Shows_back_button_on_step_2()
    {
        var cut = Render<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Steg 1", "Steg 2" })
            .Add(c => c.CurrentStep, 1)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Content")));

        var backButton = cut.Find("[aria-label='Tillbaka']");
        Assert.NotNull(backButton);
    }

    [Fact]
    public void Hides_back_button_on_first_step()
    {
        var cut = Render<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Steg 1", "Steg 2" })
            .Add(c => c.CurrentStep, 0)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Content")));

        var backButtons = cut.FindAll("[aria-label='Tillbaka']");
        Assert.Empty(backButtons);
    }
}
