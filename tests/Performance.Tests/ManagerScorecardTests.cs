using RegionHR.Performance.Domain;
using Xunit;

namespace RegionHR.Performance.Tests;

public class ManagerScorecardTests
{
    [Fact]
    public void Generera_SetsAllProperties()
    {
        var chefId = Guid.NewGuid();

        var scorecard = ManagerScorecard.Generera(
            chefId, "2026-Q1", 8, 12.5m, -1.3m, 75m, 14m);

        Assert.NotEqual(Guid.Empty, scorecard.Id);
        Assert.Equal(chefId, scorecard.ChefId);
        Assert.Equal("2026-Q1", scorecard.Period);
        Assert.Equal(8, scorecard.SpanOfControl);
        Assert.Equal(12.5m, scorecard.TeamOmsattning);
        Assert.Equal(-1.3m, scorecard.EngagementDelta);
        Assert.Equal(75m, scorecard.UtvecklingsplanFardiggrad);
        Assert.Equal(14m, scorecard.MedelTidMellanOneonone);
        Assert.True(scorecard.GenereradVid <= DateTime.UtcNow);
    }

    [Fact]
    public void Generera_WithEmptyChefId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ManagerScorecard.Generera(Guid.Empty, "2026-Q1", 5, 10, 0, 50, 14));
    }

    [Fact]
    public void Generera_WithEmptyPeriod_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ManagerScorecard.Generera(Guid.NewGuid(), "", 5, 10, 0, 50, 14));
    }
}
