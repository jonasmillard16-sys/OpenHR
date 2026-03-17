using Xunit;
using System.Xml.Linq;
using RegionHR.IntegrationHub.Adapters.Skatteverket;

namespace RegionHR.IntegrationHub.Tests;

public class AGIXmlGeneratorTests
{
    private const string AGI_NAMESPACE = "http://xmls.skatteverket.se/se/skatteverket/ai/instans/inkomstdeklaration/1.1";

    private static AGIInput CreateSingleIndividInput() => new()
    {
        Organisationsnummer = "2321000123",
        Period = "202603",
        KontaktpersonNamn = "Anna Svensson",
        KontaktpersonTelefon = "010-1234567",
        KontaktpersonEpost = "anna.svensson@region.se",
        Individer =
        [
            new AGIIndivid
            {
                Personnummer = "198501011234",
                Namn = "Erik Johansson",
                KontantBruttolonMm = 35000m,
                AvdragenSkatt = 8750m,
                SkattepliktForman = 500m,
                SkattefriForman = 0m,
                Traktamente = 0m,
                Milersattning = 0m,
                Avgiftsunderlag = 35500m,
                Arbetsgivaravgifter = 11160m,
                AnstallningFrom = new DateOnly(2020, 1, 1),
                AnstallningTom = null
            }
        ]
    };

    [Fact]
    public void Generate_SingleIndivid_ReturnsOneFile()
    {
        // Arrange
        var generator = new AGIXmlGenerator();
        var input = CreateSingleIndividInput();

        // Act
        var files = generator.Generate(input);

        // Assert
        Assert.Single(files);
        Assert.Contains("AGI_2321000123_202603_001.xml", files[0].FileName);
        Assert.NotEmpty(files[0].XmlContent);

        // Verify XML is parseable and contains the individual
        var doc = XDocument.Parse(files[0].XmlContent);
        var ns = XNamespace.Get(AGI_NAMESPACE);
        var iu = doc.Descendants(ns + "IU").ToList();
        Assert.Single(iu);
        Assert.Equal("198501011234", iu[0].Element(ns + "Personnummer")?.Value);
    }

    [Fact]
    public void Generate_Over1000Individer_SplitsIntoBatches()
    {
        // Arrange
        var generator = new AGIXmlGenerator();
        var input = new AGIInput
        {
            Organisationsnummer = "2321000123",
            Period = "202603",
            KontaktpersonNamn = "Anna Svensson",
            KontaktpersonTelefon = "010-1234567",
            KontaktpersonEpost = "anna.svensson@region.se",
            Individer = Enumerable.Range(0, 1500).Select(i => new AGIIndivid
            {
                Personnummer = $"19850101{i:D4}",
                Namn = $"Person {i}",
                KontantBruttolonMm = 30000m,
                AvdragenSkatt = 7500m,
                Avgiftsunderlag = 30000m,
                Arbetsgivaravgifter = 9420m
            }).ToList()
        };

        // Act
        var files = generator.Generate(input);

        // Assert - should split into 2 files (1000 + 500)
        Assert.Equal(2, files.Count);
        Assert.Contains("_001.xml", files[0].FileName);
        Assert.Contains("_002.xml", files[1].FileName);

        // Verify first file has 1000 individuals
        var doc1 = XDocument.Parse(files[0].XmlContent);
        var ns = XNamespace.Get(AGI_NAMESPACE);
        Assert.Equal(1000, doc1.Descendants(ns + "IU").Count());

        // Verify second file has 500 individuals
        var doc2 = XDocument.Parse(files[1].XmlContent);
        Assert.Equal(500, doc2.Descendants(ns + "IU").Count());
    }

    [Fact]
    public void Generate_ContainsAllFieldCodes()
    {
        // Arrange
        var generator = new AGIXmlGenerator();
        var input = new AGIInput
        {
            Organisationsnummer = "2321000123",
            Period = "202603",
            KontaktpersonNamn = "Test",
            KontaktpersonTelefon = "010-0000000",
            KontaktpersonEpost = "test@test.se",
            Individer =
            [
                new AGIIndivid
                {
                    Personnummer = "198501011234",
                    Namn = "Test Person",
                    KontantBruttolonMm = 35000m,
                    AvdragenSkatt = 8750m,
                    SkattepliktForman = 500m,
                    SkattefriForman = 200m,
                    Traktamente = 1500m,
                    Milersattning = 300m,
                    Avgiftsunderlag = 35500m,
                    Arbetsgivaravgifter = 11160m
                }
            ]
        };

        // Act
        var files = generator.Generate(input);
        var xml = files[0].XmlContent;
        var doc = XDocument.Parse(xml);
        var ns = XNamespace.Get(AGI_NAMESPACE);
        var iu = doc.Descendants(ns + "IU").First();

        // Assert - all field codes present when values > 0
        Assert.NotNull(iu.Element(ns + "Falt011")); // Kontant bruttolön
        Assert.NotNull(iu.Element(ns + "Falt012")); // Skatteavdrag
        Assert.NotNull(iu.Element(ns + "Falt013")); // Skattepliktig förmån
        Assert.NotNull(iu.Element(ns + "Falt018")); // Skattefri förmån
        Assert.NotNull(iu.Element(ns + "Falt050")); // Traktamente
        Assert.NotNull(iu.Element(ns + "Falt051")); // Milersättning
        Assert.NotNull(iu.Element(ns + "Avgiftsunderlag"));
        Assert.NotNull(iu.Element(ns + "Arbetsgivaravgifter"));
    }

    [Fact]
    public void Generate_HasCorrectNamespace()
    {
        // Arrange
        var generator = new AGIXmlGenerator();
        var input = CreateSingleIndividInput();

        // Act
        var files = generator.Generate(input);
        var doc = XDocument.Parse(files[0].XmlContent);

        // Assert
        var ns = XNamespace.Get(AGI_NAMESPACE);
        var root = doc.Root;
        Assert.NotNull(root);
        Assert.Equal(ns + "Arbetsgivardeklaration", root.Name);
        Assert.Equal("1.1", root.Attribute("version")?.Value);
    }
}
