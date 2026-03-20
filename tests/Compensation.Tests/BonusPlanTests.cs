using Xunit;
using RegionHR.Compensation.Domain;

namespace RegionHR.Compensation.Tests;

public class BonusPlanTests
{
    [Fact]
    public void Skapa_satter_korrekta_varden()
    {
        var plan = BonusPlan.Skapa("Q1 Bonus", BonusTyp.Individual, "Mars");

        Assert.Equal("Q1 Bonus", plan.Namn);
        Assert.Equal(BonusTyp.Individual, plan.Typ);
        Assert.Equal("Mars", plan.UtbetalningsTidpunkt);
        Assert.Equal(BonusPlanStatus.Draft, plan.Status);
        Assert.Empty(plan.Targets);
    }

    [Fact]
    public void Skapa_med_tomt_namn_kastar_exception()
    {
        Assert.Throws<ArgumentException>(() =>
            BonusPlan.Skapa("", BonusTyp.Individual));
    }

    [Fact]
    public void Statusovergang_Draft_Active_Closed()
    {
        var plan = BonusPlan.Skapa("Test", BonusTyp.Grupp);
        Assert.Equal(BonusPlanStatus.Draft, plan.Status);

        plan.Aktivera();
        Assert.Equal(BonusPlanStatus.Active, plan.Status);

        plan.Stang();
        Assert.Equal(BonusPlanStatus.Closed, plan.Status);
    }

    [Fact]
    public void LaggTillTarget_satts_korrekt()
    {
        var plan = BonusPlan.Skapa("Test", BonusTyp.Individual);
        var target = new BonusTarget
        {
            MalKPI = "Patientnojdhet",
            Vikt = 50m,
            Troskel = 70m,
            Tak = 120m
        };

        plan.LaggTillTarget(target);

        Assert.Single(plan.Targets);
        Assert.Equal(plan.Id, target.BonusPlanId);
        Assert.Equal("Patientnojdhet", plan.Targets[0].MalKPI);
    }

    [Fact]
    public void SattBerakningsModell_lagras()
    {
        var plan = BonusPlan.Skapa("Test", BonusTyp.Foretag);
        var json = """{"formula": "revenue * 0.05"}""";

        plan.SattBerakningsModell(json);

        Assert.Equal(json, plan.BerakningsModell);
    }
}
