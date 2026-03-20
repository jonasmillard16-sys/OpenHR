using Xunit;
using RegionHR.Compensation.Domain;

namespace RegionHR.Compensation.Tests;

public class CompensationPlanTests
{
    [Fact]
    public void Skapa_satter_korrekta_varden()
    {
        var plan = CompensationPlan.Skapa(
            "Lonerevision 2026",
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 12, 31),
            2_500_000m);

        Assert.Equal("Lonerevision 2026", plan.Namn);
        Assert.Equal(new DateOnly(2026, 4, 1), plan.GiltigFran);
        Assert.Equal(new DateOnly(2026, 12, 31), plan.GiltigTill);
        Assert.Equal(2_500_000m, plan.TotalBudget);
        Assert.Equal(CompensationPlanStatus.Draft, plan.Status);
    }

    [Fact]
    public void Aktivera_fran_Draft_lyckas()
    {
        var plan = CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1_000_000m);

        plan.Aktivera();

        Assert.Equal(CompensationPlanStatus.Active, plan.Status);
    }

    [Fact]
    public void Aktivera_fran_Active_kastar_exception()
    {
        var plan = CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1_000_000m);
        plan.Aktivera();

        Assert.Throws<InvalidOperationException>(() => plan.Aktivera());
    }

    [Fact]
    public void Aktivera_fran_Closed_kastar_exception()
    {
        var plan = CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1_000_000m);
        plan.Aktivera();
        plan.Stang();

        Assert.Throws<InvalidOperationException>(() => plan.Aktivera());
    }

    [Fact]
    public void Stang_fran_Active_lyckas()
    {
        var plan = CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1_000_000m);
        plan.Aktivera();

        plan.Stang();

        Assert.Equal(CompensationPlanStatus.Closed, plan.Status);
    }

    [Fact]
    public void Stang_fran_Draft_kastar_exception()
    {
        var plan = CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1_000_000m);

        Assert.Throws<InvalidOperationException>(() => plan.Stang());
    }

    [Fact]
    public void Statusovergang_Draft_Active_Closed()
    {
        var plan = CompensationPlan.Skapa("Lonerevision", new DateOnly(2026, 4, 1), new DateOnly(2026, 12, 31), 500_000m);
        Assert.Equal(CompensationPlanStatus.Draft, plan.Status);

        plan.Aktivera();
        Assert.Equal(CompensationPlanStatus.Active, plan.Status);

        plan.Stang();
        Assert.Equal(CompensationPlanStatus.Closed, plan.Status);
    }

    [Fact]
    public void Skapa_med_tomt_namn_kastar_exception()
    {
        Assert.Throws<ArgumentException>(() =>
            CompensationPlan.Skapa("", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 100m));
    }

    [Fact]
    public void Skapa_med_ogiltigt_datumintervall_kastar_exception()
    {
        Assert.Throws<ArgumentException>(() =>
            CompensationPlan.Skapa("Test", new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1), 100m));
    }

    [Fact]
    public void Skapa_med_negativ_budget_kastar_exception()
    {
        Assert.Throws<ArgumentException>(() =>
            CompensationPlan.Skapa("Test", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), -100m));
    }
}
