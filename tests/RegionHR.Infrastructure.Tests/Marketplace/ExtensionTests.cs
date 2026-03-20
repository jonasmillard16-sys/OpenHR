using RegionHR.Platform.Domain;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Marketplace;

public class ExtensionTests
{
    [Fact]
    public void Skapa_SkaparExtensionMedKorrekta_Varden()
    {
        var ext = Extension.Skapa(
            "openhr-utrustning",
            "1.0.0",
            "OpenHR Community",
            "Utrustningshantering",
            ExtensionTyp.CustomObject,
            "AGPL-3.0",
            ">=2.0.0",
            """{"customObjects":["utrustning.json"]}""");

        Assert.NotEqual(Guid.Empty, ext.Id);
        Assert.Equal("openhr-utrustning", ext.Namn);
        Assert.Equal("1.0.0", ext.Version);
        Assert.Equal("OpenHR Community", ext.Forfattare);
        Assert.Equal("Utrustningshantering", ext.Beskrivning);
        Assert.Equal(ExtensionTyp.CustomObject, ext.Typ);
        Assert.Equal("AGPL-3.0", ext.Licens);
        Assert.Equal(">=2.0.0", ext.Kompatibilitet);
        Assert.Contains("utrustning.json", ext.Innehall);
    }

    [Fact]
    public void Skapa_UtanNamn_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            Extension.Skapa("", "1.0.0", "Author", "Desc", ExtensionTyp.CustomObject, "MIT", ">=1.0.0", "{}"));
    }

    [Fact]
    public void Skapa_UtanVersion_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            Extension.Skapa("test", "", "Author", "Desc", ExtensionTyp.CustomObject, "MIT", ">=1.0.0", "{}"));
    }

    [Fact]
    public void Skapa_UtanForfattare_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            Extension.Skapa("test", "1.0.0", "", "Desc", ExtensionTyp.CustomObject, "MIT", ">=1.0.0", "{}"));
    }

    [Fact]
    public void Skapa_SattSkapadVidTillUtcNow()
    {
        var before = DateTime.UtcNow;
        var ext = Extension.Skapa("test", "1.0.0", "Author", "Desc", ExtensionTyp.Workflow, "AGPL-3.0", ">=1.0.0", "{}");
        var after = DateTime.UtcNow;

        Assert.InRange(ext.SkapadVid, before, after);
    }
}
