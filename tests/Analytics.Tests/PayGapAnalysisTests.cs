using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class PayGapAnalysisTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var reportId = Guid.NewGuid();
        var analysis = PayGapAnalysis.Skapa(
            reportId, "Sjukskoterska", 85, 25,
            33800m, 35200m, 33500m, 34800m,
            3.98m, 2.1m, "{}");

        Assert.NotEqual(Guid.Empty, analysis.Id);
        Assert.Equal(reportId, analysis.PayTransparencyReportId);
        Assert.Equal("Sjukskoterska", analysis.Befattningskategori);
        Assert.Equal(85, analysis.AntalKvinnor);
        Assert.Equal(25, analysis.AntalMan);
        Assert.Equal(33800m, analysis.MedelLonKvinnor);
        Assert.Equal(35200m, analysis.MedelLonMan);
        Assert.Equal(33500m, analysis.MedianLonKvinnor);
        Assert.Equal(34800m, analysis.MedianLonMan);
        Assert.Equal(3.98m, analysis.OjusteratGapProcent);
        Assert.Equal(2.1m, analysis.JusteratGapProcent);
    }

    [Fact]
    public void Kraver5ProcentUtredning_TrueWhenOjusteratOver5()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Lakare", 40, 55,
            58000m, 62000m, 57500m, 61000m,
            6.45m, null, "{}");

        Assert.True(analysis.Kraver5ProcentUtredning);
    }

    [Fact]
    public void Kraver5ProcentUtredning_TrueWhenBothOver5()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Lakare", 40, 55,
            58000m, 62000m, 57500m, 61000m,
            6.45m, 5.5m, "{}");

        Assert.True(analysis.Kraver5ProcentUtredning);
    }

    [Fact]
    public void Kraver5ProcentUtredning_FalseWhenJusteratUnder5()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Lakare", 40, 55,
            58000m, 62000m, 57500m, 61000m,
            6.45m, 4.8m, "{}");

        Assert.False(analysis.Kraver5ProcentUtredning);
    }

    [Fact]
    public void Kraver5ProcentUtredning_FalseWhenOjusteratUnder5()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Underskoterska", 120, 30,
            27800m, 28200m, 27500m, 28000m,
            1.42m, 0.8m, "{}");

        Assert.False(analysis.Kraver5ProcentUtredning);
    }

    [Fact]
    public void LaggTillKohort_AddsToCollection()
    {
        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Test", 10, 10,
            30000m, 31000m, 30000m, 31000m,
            3.23m, 2.0m, "{}");

        analysis.LaggTillKohort(PayGapCohort.Skapa(analysis.Id, "Under 30", 5, 2.0m, -0.5m));
        analysis.LaggTillKohort(PayGapCohort.Skapa(analysis.Id, "30-50", 10, 4.0m, 0.2m));

        Assert.Equal(2, analysis.Kohorter.Count);
    }

    [Fact]
    public void Skapa_WithCohorts_IncludesThem()
    {
        var analysisId = Guid.NewGuid();
        var kohorter = new List<PayGapCohort>
        {
            PayGapCohort.Skapa(analysisId, "K1", 10, 2.5m),
            PayGapCohort.Skapa(analysisId, "K2", 15, 3.1m)
        };

        var analysis = PayGapAnalysis.Skapa(
            Guid.NewGuid(), "Test", 15, 10,
            30000m, 31000m, 30000m, 31000m,
            3.23m, 2.0m, "{}", kohorter);

        Assert.Equal(2, analysis.Kohorter.Count);
    }
}
