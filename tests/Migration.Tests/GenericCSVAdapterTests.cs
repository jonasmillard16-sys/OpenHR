using RegionHR.Migration.Adapters;
using RegionHR.Migration.Domain;
using Xunit;

namespace RegionHR.Migration.Tests;

public class GenericCSVAdapterTests
{
    [Fact]
    public void Source_ArGenericCSV()
    {
        var adapter = new GenericCSVAdapter();
        Assert.Equal(SourceSystem.GenericCSV, adapter.Source);
    }

    [Fact]
    public async Task ParseAsync_MedHeader_ParserarKorrekt()
    {
        var csv = "Name,Age,Email\nAnna,35,anna@test.se\nErik,30,erik@test.se\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        var adapter = new GenericCSVAdapter { HasHeader = true, Separator = ',' };

        var result = await adapter.ParseAsync(stream);

        Assert.Equal(2, result.Records.Count);
        Assert.Equal("Anna", result.Records[0].Fields["Name"]);
        Assert.Equal("35", result.Records[0].Fields["Age"]);
        Assert.Equal("anna@test.se", result.Records[0].Fields["Email"]);
    }

    [Fact]
    public async Task ParseAsync_UtanHeader_AnvanderColumnIndex()
    {
        var csv = "Anna,35,anna@test.se\nErik,30,erik@test.se\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        var adapter = new GenericCSVAdapter { HasHeader = false, Separator = ',' };

        var result = await adapter.ParseAsync(stream);

        Assert.Equal(2, result.Records.Count);
        Assert.Equal("Anna", result.Records[0].Fields["Column0"]);
        Assert.Equal("35", result.Records[0].Fields["Column1"]);
    }

    [Fact]
    public async Task ParseAsync_MedSemikolon_ParserarKorrekt()
    {
        var csv = "Namn;Lon\nAnna;35000\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        var adapter = new GenericCSVAdapter { HasHeader = true, Separator = ';' };

        var result = await adapter.ParseAsync(stream);

        Assert.Single(result.Records);
        Assert.Equal("Anna", result.Records[0].Fields["Namn"]);
        Assert.Equal("35000", result.Records[0].Fields["Lon"]);
    }

    [Fact]
    public async Task ParseAsync_TomFil_GerVarning()
    {
        using var stream = new MemoryStream(""u8.ToArray());
        var adapter = new GenericCSVAdapter();

        var result = await adapter.ParseAsync(stream);

        Assert.Empty(result.Records);
        Assert.Contains(result.Warnings, w => w.Contains("Tom fil"));
    }

    [Fact]
    public void GetDefaultMappings_ReturnerarTomLista()
    {
        var adapter = new GenericCSVAdapter();
        var mappings = adapter.GetDefaultMappings();
        Assert.Empty(mappings);
    }
}
