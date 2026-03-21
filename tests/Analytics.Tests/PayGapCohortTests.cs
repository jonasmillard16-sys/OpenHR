using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class PayGapCohortTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var cohort = PayGapCohort.Skapa(analysisId, "Alder: Under 30", 22, 2.1m, -0.5m);

        Assert.NotEqual(Guid.Empty, cohort.Id);
        Assert.Equal(analysisId, cohort.PayGapAnalysisId);
        Assert.Equal("Alder: Under 30", cohort.KohortNamn);
        Assert.Equal(22, cohort.AntalAnstallda);
        Assert.Equal(2.1m, cohort.GapProcent);
        Assert.Equal(-0.5m, cohort.TrendFranForraAret);
    }

    [Fact]
    public void Skapa_WithoutTrend_TrendIsNull()
    {
        var cohort = PayGapCohort.Skapa(Guid.NewGuid(), "Test", 10, 3.5m);

        Assert.Null(cohort.TrendFranForraAret);
    }

    [Fact]
    public void Skapa_NegativeGap_IndicatesWomenEarnMore()
    {
        // Negative gap means women earn more than men in this cohort
        var cohort = PayGapCohort.Skapa(Guid.NewGuid(), "Alder: 50+", 15, -1.5m, 0.3m);

        Assert.Equal(-1.5m, cohort.GapProcent);
        Assert.Equal(0.3m, cohort.TrendFranForraAret);
    }

    [Fact]
    public void Skapa_MultipleCohorts_HaveUniqueIds()
    {
        var analysisId = Guid.NewGuid();
        var c1 = PayGapCohort.Skapa(analysisId, "K1", 10, 2.0m);
        var c2 = PayGapCohort.Skapa(analysisId, "K2", 15, 3.0m);
        var c3 = PayGapCohort.Skapa(analysisId, "K3", 20, 4.0m);

        Assert.NotEqual(c1.Id, c2.Id);
        Assert.NotEqual(c2.Id, c3.Id);
        Assert.NotEqual(c1.Id, c3.Id);
    }
}
