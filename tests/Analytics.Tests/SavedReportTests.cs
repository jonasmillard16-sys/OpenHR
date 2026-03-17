using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class SavedReportTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var queryDef = """{"entity":"Employee","fields":["Fornamn","Efternamn"],"filters":[]}""";
        var report = SavedReport.Skapa(userId, "Personalrapport", "Alla anställda", queryDef, "table");

        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(userId, report.SkapadAvId);
        Assert.Equal("Personalrapport", report.Namn);
        Assert.Equal("Alla anställda", report.Beskrivning);
        Assert.Equal(queryDef, report.QueryDefinition);
        Assert.Equal("table", report.Visualisering);
        Assert.False(report.ArDelad);
        Assert.Null(report.SenastKordVid);
    }

    [Fact]
    public void Uppdatera_ChangesQueryDefinitionAndVisualisering()
    {
        var report = SavedReport.Skapa(Guid.NewGuid(), "Test", "Desc", "{}", "table");
        var newQuery = """{"entity":"Employee","fields":["Fornamn"],"filters":[{"field":"ArAktiv","value":true}]}""";

        report.Uppdatera(newQuery, "bar");

        Assert.Equal(newQuery, report.QueryDefinition);
        Assert.Equal("bar", report.Visualisering);
    }

    [Fact]
    public void Uppdatera_KeepsVisualisering_WhenNull()
    {
        var report = SavedReport.Skapa(Guid.NewGuid(), "Test", "Desc", "{}", "pie");

        report.Uppdatera("""{"new":"query"}""");

        Assert.Equal("pie", report.Visualisering);
    }

    [Fact]
    public void MarkeraSomKord_SetsSenastKordVid()
    {
        var report = SavedReport.Skapa(Guid.NewGuid(), "Test", "Desc", "{}");
        Assert.Null(report.SenastKordVid);

        var before = DateTime.UtcNow;
        report.MarkeraSomKord();
        var after = DateTime.UtcNow;

        Assert.NotNull(report.SenastKordVid);
        Assert.InRange(report.SenastKordVid!.Value, before, after);
    }

    [Fact]
    public void ToggleDelad_TogglesArDelad()
    {
        var report = SavedReport.Skapa(Guid.NewGuid(), "Test", "Desc", "{}");
        Assert.False(report.ArDelad);

        report.ToggleDelad();
        Assert.True(report.ArDelad);

        report.ToggleDelad();
        Assert.False(report.ArDelad);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var report = SavedReport.Skapa(Guid.NewGuid(), "Test", "Desc", "{}");
        var after = DateTime.UtcNow;

        Assert.InRange(report.SkapadVid, before, after);
    }
}
