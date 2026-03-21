using RegionHR.Performance.Domain;
using Xunit;

namespace RegionHR.Performance.Tests;

public class CoachingNudgeTests
{
    [Fact]
    public void Skapa_SetsDefaults()
    {
        var chefId = Guid.NewGuid();

        var nudge = CoachingNudge.Skapa(chefId, "MissedOneOnOne", "Du har inte haft 1:1 med Anna på 30 dagar.");

        Assert.NotEqual(Guid.Empty, nudge.Id);
        Assert.Equal(chefId, nudge.ChefId);
        Assert.Equal("MissedOneOnOne", nudge.Typ);
        Assert.Equal("Du har inte haft 1:1 med Anna på 30 dagar.", nudge.Meddelande);
        Assert.False(nudge.ArLast);
    }

    [Fact]
    public void Skapa_WithEmptyChefId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CoachingNudge.Skapa(Guid.Empty, "HighTurnover", "Test"));
    }

    [Fact]
    public void Skapa_WithEmptyTyp_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CoachingNudge.Skapa(Guid.NewGuid(), "", "Test"));
    }

    [Fact]
    public void Skapa_WithEmptyMeddelande_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CoachingNudge.Skapa(Guid.NewGuid(), "HighTurnover", ""));
    }

    [Fact]
    public void MarkeraSomLast_SetsArLast()
    {
        var nudge = CoachingNudge.Skapa(Guid.NewGuid(), "LowEngagement", "Engagemanget har sjunkit.");

        nudge.MarkeraSomLast();

        Assert.True(nudge.ArLast);
    }

    [Fact]
    public void MarkeraSomLast_IdempotentWhenAlreadyRead()
    {
        var nudge = CoachingNudge.Skapa(Guid.NewGuid(), "LowEngagement", "Test");
        nudge.MarkeraSomLast();

        nudge.MarkeraSomLast(); // should not throw

        Assert.True(nudge.ArLast);
    }
}
