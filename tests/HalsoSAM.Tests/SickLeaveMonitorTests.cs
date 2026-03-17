using RegionHR.HalsoSAM.Domain;
using RegionHR.HalsoSAM.Services;
using Xunit;

namespace RegionHR.HalsoSAM.Tests;

public class SickLeaveMonitorTests
{
    private readonly SickLeaveMonitor _monitor = new();

    [Fact]
    public void SexEllerFlerTillfallen_TriggrarSexTillfallenTolvManader()
    {
        // Arrange: 6 sjuktillfällen de senaste 12 månaderna
        var perioder = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-i - 1));
                return new SjukfranvaroPeriod
                {
                    StartDatum = start,
                    SlutDatum = start.AddDays(2) // 3 dagars sjukfrånvaro vardera
                };
            })
            .ToList();

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert
        Assert.Equal(RehabTrigger.SexTillfallenTolvManader, result);
    }

    [Fact]
    public void FjortonEllerFlerSammanhangandeDagar_TriggrarFjortonDagar()
    {
        // Arrange: en period på 15 dagar
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-20));
        var perioder = new List<SjukfranvaroPeriod>
        {
            new() { StartDatum = start, SlutDatum = start.AddDays(14) } // 15 dagar
        };

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert
        Assert.Equal(RehabTrigger.FjortonSammanhangandeDagar, result);
    }

    [Fact]
    public void MonsterDetekterat_MerAn50ProcentSammaVeckodag()
    {
        // Arrange: 5 tillfällen, 3 av dem börjar på måndag (>50%)
        var perioder = new List<SjukfranvaroPeriod>();

        // Hitta nästa måndag bakåt
        var today = DateTime.Today;
        var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        for (int i = 0; i < 3; i++)
        {
            var monday = DateOnly.FromDateTime(today.AddDays(-daysUntilMonday - (i * 7 + 7)));
            perioder.Add(new SjukfranvaroPeriod
            {
                StartDatum = monday,
                SlutDatum = monday // 1 dag
            });
        }

        // Lägg till 2 som inte är måndag
        var tuesday = DateOnly.FromDateTime(today.AddMonths(-5));
        while (tuesday.DayOfWeek != DayOfWeek.Tuesday)
            tuesday = tuesday.AddDays(1);
        perioder.Add(new SjukfranvaroPeriod { StartDatum = tuesday, SlutDatum = tuesday });

        var wednesday = DateOnly.FromDateTime(today.AddMonths(-6));
        while (wednesday.DayOfWeek != DayOfWeek.Wednesday)
            wednesday = wednesday.AddDays(1);
        perioder.Add(new SjukfranvaroPeriod { StartDatum = wednesday, SlutDatum = wednesday });

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert
        Assert.Equal(RehabTrigger.MonsterDetekterat, result);
    }

    [Fact]
    public void UnderTrosklar_IngenTrigger()
    {
        // Arrange: 3 tillfällen (under 6), alla korta (under 14 dagar), olika veckodagar
        var perioder = new List<SjukfranvaroPeriod>
        {
            new()
            {
                StartDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                SlutDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2).AddDays(2))
            },
            new()
            {
                StartDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4)),
                SlutDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4).AddDays(1))
            },
            new()
            {
                StartDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
                SlutDatum = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6).AddDays(3))
            }
        };

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TomInput_IngenTrigger()
    {
        // Arrange
        var perioder = new List<SjukfranvaroPeriod>();

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FemTillfallen_UnderSexGrans_IngenSexTillfallenTrigger()
    {
        // Arrange: exakt 5 tillfällen (under gränsen på 6)
        var perioder = Enumerable.Range(0, 5)
            .Select(i =>
            {
                var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-i - 1));
                return new SjukfranvaroPeriod
                {
                    StartDatum = start,
                    SlutDatum = start.AddDays(1)
                };
            })
            .ToList();

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert: ska inte trigga SexTillfällen (bara 5 st)
        Assert.True(result is null or not RehabTrigger.SexTillfallenTolvManader
            || result != RehabTrigger.SexTillfallenTolvManader);
    }

    [Fact]
    public void ExaktTrettonDagar_IngenFjortonDagarTrigger()
    {
        // Arrange: perioder under 14 dagar, spridda på olika veckodagar för att undvika mönsterdetektering
        var today = DateTime.Today;
        // Skapa 3 korta perioder som startar på olika veckodagar
        var monday = DateOnly.FromDateTime(today.AddDays(-60));
        while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(1);
        var wednesday = DateOnly.FromDateTime(today.AddDays(-40));
        while (wednesday.DayOfWeek != DayOfWeek.Wednesday) wednesday = wednesday.AddDays(1);
        var friday = DateOnly.FromDateTime(today.AddDays(-20));
        while (friday.DayOfWeek != DayOfWeek.Friday) friday = friday.AddDays(1);

        var perioder = new List<SjukfranvaroPeriod>
        {
            new() { StartDatum = monday, SlutDatum = monday.AddDays(12) },       // 13 dagar
            new() { StartDatum = wednesday, SlutDatum = wednesday.AddDays(10) },  // 11 dagar
            new() { StartDatum = friday, SlutDatum = friday.AddDays(8) }          // 9 dagar
        };

        // Act
        var result = _monitor.Analysera(perioder);

        // Assert: ingen period >= 14 dagar, och alla under trösklar
        Assert.Null(result);
    }
}
