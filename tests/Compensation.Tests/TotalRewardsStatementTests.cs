using Xunit;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Tests;

public class TotalRewardsStatementTests
{
    [Fact]
    public void Generera_beraknar_totalkompensation_korrekt()
    {
        var anstallId = EmployeeId.New();
        decimal grundLon = 38500m;
        decimal tillagg = 1925m;
        decimal pension = 1732.5m;
        decimal forsakringar = 800m;
        decimal formaner = 2500m;
        decimal agAvgifter = 12094.7m;

        var statement = TotalRewardsStatement.Generera(
            anstallId, 2026, grundLon, tillagg, pension, forsakringar, formaner, agAvgifter);

        Assert.Equal(anstallId, statement.AnstallId);
        Assert.Equal(2026, statement.Ar);
        Assert.Equal(grundLon, statement.GrundLon);
        Assert.Equal(tillagg, statement.Tillagg);
        Assert.Equal(pension, statement.Pension);
        Assert.Equal(forsakringar, statement.Forsakringar);
        Assert.Equal(formaner, statement.Formaner);
        Assert.Equal(agAvgifter, statement.AGAvgifter);

        decimal forvantadTotal = grundLon + tillagg + pension + forsakringar + formaner + agAvgifter;
        Assert.Equal(forvantadTotal, statement.TotalKompensation);
    }

    [Fact]
    public void Generera_satter_genereringsdatum()
    {
        var fore = DateTime.UtcNow;
        var statement = TotalRewardsStatement.Generera(
            EmployeeId.New(), 2026, 35000m, 1000m, 1500m, 500m, 2000m, 11000m);
        var efter = DateTime.UtcNow;

        Assert.True(statement.GenereradVid >= fore && statement.GenereradVid <= efter);
    }

    [Fact]
    public void Generera_med_nollvarden_ger_noll_total()
    {
        var statement = TotalRewardsStatement.Generera(
            EmployeeId.New(), 2026, 0m, 0m, 0m, 0m, 0m, 0m);

        Assert.Equal(0m, statement.TotalKompensation);
    }

    [Fact]
    public void Generera_flera_ganger_ger_unika_id()
    {
        var id = EmployeeId.New();
        var s1 = TotalRewardsStatement.Generera(id, 2025, 30000m, 0m, 0m, 0m, 0m, 0m);
        var s2 = TotalRewardsStatement.Generera(id, 2026, 35000m, 0m, 0m, 0m, 0m, 0m);

        Assert.NotEqual(s1.Id, s2.Id);
    }
}
