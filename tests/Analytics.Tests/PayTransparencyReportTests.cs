using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class PayTransparencyReportTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025-01-01 till 2025-12-31");

        Assert.NotEqual(Guid.Empty, rapport.Id);
        Assert.Equal(2025, rapport.Ar);
        Assert.Equal("2025-01-01 till 2025-12-31", rapport.RapportPeriod);
        Assert.Equal("Draft", rapport.Status);
        Assert.Equal(0, rapport.TotalAnstallda);
    }

    [Fact]
    public void Berakna_SetsStatusToCalculated()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025");

        var analyser = new List<PayGapAnalysis>
        {
            PayGapAnalysis.Skapa(rapport.Id, "Sjukskoterska", 10, 5,
                33000m, 35000m, 33000m, 35000m, 5.71m, 3.5m, "{}")
        };

        rapport.Berakna(15, 5.71m, 4.50m, "{}", analyser);

        Assert.Equal("Calculated", rapport.Status);
        Assert.Equal(15, rapport.TotalAnstallda);
        Assert.Equal(5.71m, rapport.KonsGapProcent);
        Assert.Equal(4.50m, rapport.MedianGapProcent);
        Assert.Single(rapport.Analyser);
    }

    [Fact]
    public void Publicera_SetsStatusToPublished()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025");
        rapport.Berakna(10, 3.0m, 2.5m, "{}", []);

        rapport.Publicera();

        Assert.Equal("Published", rapport.Status);
        Assert.NotNull(rapport.PubliceradVid);
    }

    [Fact]
    public void Publicera_ThrowsIfNotCalculated()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025");

        Assert.Throws<InvalidOperationException>(() => rapport.Publicera());
    }

    [Fact]
    public void Berakna_ThrowsIfAlreadyPublished()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025");
        rapport.Berakna(10, 3.0m, 2.5m, "{}", []);
        rapport.Publicera();

        Assert.Throws<InvalidOperationException>(() =>
            rapport.Berakna(20, 4.0m, 3.0m, "{}", []));
    }

    [Fact]
    public void Berakna_CanRecalculateWhenCalculatedButNotPublished()
    {
        var rapport = PayTransparencyReport.Skapa(2025, "2025");
        rapport.Berakna(10, 3.0m, 2.5m, "{}", []);

        // Should not throw
        rapport.Berakna(20, 4.0m, 3.0m, "{}", []);

        Assert.Equal(20, rapport.TotalAnstallda);
        Assert.Equal(4.0m, rapport.KonsGapProcent);
    }
}
