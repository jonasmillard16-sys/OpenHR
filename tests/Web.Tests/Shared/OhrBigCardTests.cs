using Bunit;
using RegionHR.Web.Components.Shared;
using Xunit;

namespace RegionHR.Web.Tests.Shared;

public class OhrBigCardTests : BunitContext
{
    [Fact]
    public void Renders_title_and_description()
    {
        var cut = Render<OhrBigCard>(p => p
            .Add(c => c.Icon, "\U0001f4c5")
            .Add(c => c.Title, "Mitt schema")
            .Add(c => c.Description, "Se när du jobbar")
            .Add(c => c.Href, "/minsida/schema"));

        var markup = cut.Markup;
        Assert.Contains("Mitt schema", markup);
        Assert.Contains("Se när du jobbar", markup);
    }

    [Fact]
    public void Renders_badge_value_when_provided()
    {
        var cut = Render<OhrBigCard>(p => p
            .Add(c => c.Icon, "\U0001f334")
            .Add(c => c.Title, "Jag vill ha ledigt")
            .Add(c => c.BadgeValue, "23 dagar kvar")
            .Add(c => c.Href, "/minsida/ledighet"));

        Assert.Contains("23 dagar kvar", cut.Markup);
    }

    [Fact]
    public void Has_link_role_and_aria_label()
    {
        var cut = Render<OhrBigCard>(p => p
            .Add(c => c.Icon, "\U0001f637")
            .Add(c => c.Title, "Jag är sjuk")
            .Add(c => c.Href, "/minsida/sjukanmalan"));

        var paper = cut.Find("[role='link']");
        Assert.Equal("Jag är sjuk", paper.GetAttribute("aria-label"));
    }
}
