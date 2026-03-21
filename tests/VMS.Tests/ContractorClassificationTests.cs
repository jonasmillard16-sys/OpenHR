using Xunit;
using RegionHR.VMS.Domain;

namespace RegionHR.VMS.Tests;

public class ContractorClassificationTests
{
    [Fact]
    public void Bedom_SkaparKlassificering()
    {
        var workerId = Guid.NewGuid();
        var classification = ContractorClassification.Bedöm(
            workerId, "Contractor", "Low",
            """{"control":0,"tools":0}""", "Anna HR");

        Assert.NotEqual(Guid.Empty, classification.Id);
        Assert.Equal(workerId, classification.ContingentWorkerId);
        Assert.Equal("Contractor", classification.BedömningsResultat);
        Assert.Equal("Low", classification.RiskNivå);
        Assert.Equal("Anna HR", classification.BedömdAv);
    }

    [Fact]
    public void Bedom_KastarFel_ForOgiltigtResultat()
    {
        Assert.Throws<ArgumentException>(() =>
            ContractorClassification.Bedöm(Guid.NewGuid(), "Invalid", "Low", "{}", "Test"));
    }

    [Fact]
    public void Bedom_KastarFel_ForOgiltigRiskniva()
    {
        Assert.Throws<ArgumentException>(() =>
            ContractorClassification.Bedöm(Guid.NewGuid(), "Contractor", "Invalid", "{}", "Test"));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, "Contractor", "Low")]
    [InlineData(2, 2, 2, 2, 2, "Employee", "High")]
    [InlineData(1, 1, 1, 1, 0, "Unclear", "Medium")]
    [InlineData(1, 1, 1, 1, 1, "Unclear", "Medium")]
    [InlineData(2, 2, 2, 1, 1, "Employee", "High")]
    public void BeraknaRisk_ReturnererKorrektResultat(
        int kontroll, int verktyg, int ekonomiskt, int integration, int varaktighet,
        string forväntatResultat, string forvantadRisk)
    {
        var (resultat, risk) = ContractorClassification.BeräknaRisk(kontroll, verktyg, ekonomiskt, integration, varaktighet);

        Assert.Equal(forväntatResultat, resultat);
        Assert.Equal(forvantadRisk, risk);
    }
}
