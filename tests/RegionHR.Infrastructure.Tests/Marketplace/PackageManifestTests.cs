using System.IO.Compression;
using System.Text;
using System.Text.Json;
using RegionHR.Infrastructure.Services;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Marketplace;

public class PackageManifestTests
{
    [Fact]
    public void PackageManifest_Deserialisering_LasKorrekta_Varden()
    {
        var json = """
        {
            "name": "openhr-utrustning",
            "version": "1.0.0",
            "description": "Utrustningshantering",
            "author": "OpenHR Community",
            "license": "AGPL-3.0",
            "compatibility": ">=2.0.0",
            "contents": {
                "customObjects": ["utrustning.json"],
                "workflows": [],
                "reports": []
            }
        }
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var manifest = JsonSerializer.Deserialize<PackageManifest>(json, options);

        Assert.NotNull(manifest);
        Assert.Equal("openhr-utrustning", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal("Utrustningshantering", manifest.Description);
        Assert.Equal("OpenHR Community", manifest.Author);
        Assert.Equal("AGPL-3.0", manifest.License);
        Assert.Equal(">=2.0.0", manifest.Compatibility);
        Assert.Single(manifest.Contents.CustomObjects);
        Assert.Equal("utrustning.json", manifest.Contents.CustomObjects[0]);
        Assert.Empty(manifest.Contents.Workflows);
        Assert.Empty(manifest.Contents.Reports);
    }

    [Fact]
    public void PackageManifest_Serialisering_ProducerGiltigJson()
    {
        var manifest = new PackageManifest
        {
            Name = "test-ext",
            Version = "2.0.0",
            Description = "Test",
            Author = "Author",
            License = "MIT",
            Compatibility = ">=1.0.0",
            Contents = new PackageContents
            {
                CustomObjects = ["obj.json"],
                Workflows = ["flow.json"],
                Reports = []
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(manifest, options);
        var parsed = JsonDocument.Parse(json);

        Assert.Equal("test-ext", parsed.RootElement.GetProperty("name").GetString());
        Assert.Equal("2.0.0", parsed.RootElement.GetProperty("version").GetString());
        Assert.Equal(1, parsed.RootElement.GetProperty("contents").GetProperty("customObjects").GetArrayLength());
        Assert.Equal(1, parsed.RootElement.GetProperty("contents").GetProperty("workflows").GetArrayLength());
        Assert.Equal(0, parsed.RootElement.GetProperty("contents").GetProperty("reports").GetArrayLength());
    }

    [Fact]
    public void PackageContents_DefaultAr_TommaListor()
    {
        var contents = new PackageContents();

        Assert.Empty(contents.CustomObjects);
        Assert.Empty(contents.Workflows);
        Assert.Empty(contents.Reports);
    }

    [Fact]
    public void CreateValidZipWithManifest_CanBeReadBack()
    {
        var manifest = """
        {
            "name": "test-roundtrip",
            "version": "1.0.0",
            "description": "Test",
            "author": "Tester",
            "license": "MIT",
            "compatibility": ">=1.0.0",
            "contents": {
                "customObjects": [],
                "workflows": ["flow.json"],
                "reports": []
            }
        }
        """;

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("manifest.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(manifest);
        }

        ms.Position = 0;
        using var readArchive = new ZipArchive(ms, ZipArchiveMode.Read);
        var manifestEntry = readArchive.GetEntry("manifest.json");

        Assert.NotNull(manifestEntry);

        using var reader = new StreamReader(manifestEntry!.Open());
        var content = reader.ReadToEnd();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var parsed = JsonSerializer.Deserialize<PackageManifest>(content, options);

        Assert.NotNull(parsed);
        Assert.Equal("test-roundtrip", parsed.Name);
        Assert.Single(parsed.Contents.Workflows);
    }
}
