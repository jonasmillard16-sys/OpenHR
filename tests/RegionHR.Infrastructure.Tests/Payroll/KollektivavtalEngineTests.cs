using RegionHR.Infrastructure.Payroll;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Payroll;

public class KollektivavtalEngineTests
{
    private readonly KollektivavtalEngine _engine = new();

    [Fact]
    public void BeraknaOB_Natt_ReturnsNattRate()
    {
        var result = _engine.BeraknaOB(
            new DateTime(2026, 3, 18, 22, 0, 0),
            new DateTime(2026, 3, 19, 6, 0, 0));
        Assert.Equal("Natt", result.Typ);
        Assert.True(result.Belopp > 0);
    }

    [Fact]
    public void BeraknaOB_Dag_NoOB()
    {
        var result = _engine.BeraknaOB(
            new DateTime(2026, 3, 18, 7, 0, 0),
            new DateTime(2026, 3, 18, 16, 0, 0));
        Assert.Contains("Dag", result.Typ);
        Assert.Equal(0m, result.Belopp);
    }

    [Fact]
    public void KontrolleraVila_Enough_OK()
    {
        var result = _engine.KontrolleraVila(
            new DateTime(2026, 3, 18, 16, 0, 0),
            new DateTime(2026, 3, 19, 7, 0, 0));
        Assert.True(result.Godkand);
    }

    [Fact]
    public void KontrolleraVila_TooShort_NotOK()
    {
        var result = _engine.KontrolleraVila(
            new DateTime(2026, 3, 18, 22, 0, 0),
            new DateTime(2026, 3, 19, 6, 0, 0));
        Assert.False(result.Godkand);
    }

    [Fact]
    public void BeraknaSemester_Under40_25Days()
    {
        var result = _engine.BeraknaSemester(35, 24);
        Assert.Equal(25, result.ArligaDagar);
    }

    [Fact]
    public void BeraknaSemester_Over50_32Days()
    {
        var result = _engine.BeraknaSemester(55, 120);
        Assert.Equal(32, result.ArligaDagar);
    }
}
