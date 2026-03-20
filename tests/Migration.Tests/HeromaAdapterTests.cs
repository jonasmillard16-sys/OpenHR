using RegionHR.Migration.Adapters;
using RegionHR.Migration.Domain;
using Xunit;

namespace RegionHR.Migration.Tests;

public class HeromaAdapterTests
{
    private readonly HeromaAdapter _adapter = new();

    [Fact]
    public void Source_ArHEROMA()
    {
        Assert.Equal(SourceSystem.HEROMA, _adapter.Source);
    }

    [Fact]
    public async Task ParseAsync_ParserarTreRader()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample-heroma.csv"));

        var result = await _adapter.ParseAsync(stream);

        Assert.Equal(3, result.Records.Count);
        Assert.Equal(3, result.TotalRows);
    }

    [Fact]
    public async Task ParseAsync_MapparFaltKorrekt()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample-heroma.csv"));

        var result = await _adapter.ParseAsync(stream);

        var first = result.Records[0];
        Assert.Equal("Employee", first.EntityType);
        Assert.Equal("198501151234", first.Fields["Personnummer"]);
        Assert.Equal("Anna", first.Fields["Fornamn"]);
        Assert.Equal("Svensson", first.Fields["Efternamn"]);
        Assert.Equal("Tillsvidare", first.Fields["Anstallningsform"]);
        Assert.Equal("AB", first.Fields["Kollektivavtal"]);
        Assert.Equal("35000", first.Fields["Manadslon"]);
        Assert.Equal("VE001", first.Fields["Enhetskod"]);
    }

    [Fact]
    public async Task ParseAsync_ParserarAllaRaderKorrekt()
    {
        using var stream = File.OpenRead(Path.Combine("TestData", "sample-heroma.csv"));

        var result = await _adapter.ParseAsync(stream);

        var maria = result.Records[2];
        Assert.Equal("198712301890", maria.Fields["Personnummer"]);
        Assert.Equal("Maria", maria.Fields["Fornamn"]);
        Assert.Equal("Lindberg", maria.Fields["Efternamn"]);
        Assert.Equal("42000", maria.Fields["Manadslon"]);
    }

    [Fact]
    public async Task ParseAsync_TomFil_GerVarning()
    {
        using var stream = new MemoryStream(""u8.ToArray());

        var result = await _adapter.ParseAsync(stream);

        Assert.Empty(result.Records);
        Assert.Contains(result.Warnings, w => w.Contains("Tom fil"));
    }

    [Fact]
    public async Task ParseAsync_SaknadeKolumner_GerVarning()
    {
        var csv = "PERSNR;FNAMN\n198501151234;Anna\n";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _adapter.ParseAsync(stream);

        Assert.Single(result.Records);
        Assert.True(result.Warnings.Count > 0, "Borde ge varningar om saknade kolumner");
    }

    [Fact]
    public void GetDefaultMappings_ReturnerarMappningar()
    {
        var mappings = _adapter.GetDefaultMappings();

        Assert.NotEmpty(mappings);
        Assert.Contains(mappings, m => m.KallFalt == "PERSNR" && m.MalFalt == "Personnummer");
        Assert.Contains(mappings, m => m.KallFalt == "MANLON" && m.MalFalt == "Manadslon");
    }
}
