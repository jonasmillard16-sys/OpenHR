using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class DashboardTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var agarId = Guid.NewGuid();
        var layout = """[{"widget":"chart","position":{"x":0,"y":0,"w":6,"h":4},"reportId":"abc"}]""";
        var dashboard = Dashboard.Skapa(agarId, "Min dashboard", layout);

        Assert.NotEqual(Guid.Empty, dashboard.Id);
        Assert.Equal(agarId, dashboard.AgarId);
        Assert.Equal("Min dashboard", dashboard.Namn);
        Assert.Equal(layout, dashboard.Layout);
        Assert.False(dashboard.ArDelad);
    }

    [Fact]
    public void Skapa_DefaultsLayoutToEmptyArray()
    {
        var dashboard = Dashboard.Skapa(Guid.NewGuid(), "Test");

        Assert.Equal("[]", dashboard.Layout);
    }

    [Fact]
    public void UppdateraLayout_ChangesLayout()
    {
        var dashboard = Dashboard.Skapa(Guid.NewGuid(), "Test");
        var newLayout = """[{"widget":"kpi","position":{"x":0,"y":0}}]""";

        dashboard.UppdateraLayout(newLayout);

        Assert.Equal(newLayout, dashboard.Layout);
    }

    [Fact]
    public void ToggleDelad_TogglesArDelad()
    {
        var dashboard = Dashboard.Skapa(Guid.NewGuid(), "Test");
        Assert.False(dashboard.ArDelad);

        dashboard.ToggleDelad();
        Assert.True(dashboard.ArDelad);

        dashboard.ToggleDelad();
        Assert.False(dashboard.ArDelad);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var dashboard = Dashboard.Skapa(Guid.NewGuid(), "Test");
        var after = DateTime.UtcNow;

        Assert.InRange(dashboard.SkapadVid, before, after);
    }
}
