using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.SharedKernel.Tests;

public class DateRangeTests
{
    [Fact]
    public void Contains_DatumInomPeriod_ReturnerarTrue()
    {
        var range = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
        Assert.True(range.Contains(new DateOnly(2025, 6, 15)));
    }

    [Fact]
    public void Contains_DatumUtanforPeriod_ReturnerarFalse()
    {
        var range = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
        Assert.False(range.Contains(new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void OpenEnded_DatumEfterStart_ReturnerarTrue()
    {
        var range = DateRange.Infinite(new DateOnly(2025, 1, 1));
        Assert.True(range.Contains(new DateOnly(2099, 12, 31)));
        Assert.True(range.IsOpenEnded);
    }

    [Fact]
    public void Overlaps_OverlappandePerioder_ReturnerarTrue()
    {
        var a = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 6, 30));
        var b = new DateRange(new DateOnly(2025, 3, 1), new DateOnly(2025, 9, 30));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void SlutdatumForeStartdatum_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            new DateRange(new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1)));
    }

    [Fact]
    public void SingleDay_EnDag()
    {
        var range = DateRange.SingleDay(new DateOnly(2025, 3, 15));
        Assert.Equal(1, range.DurationInDays);
    }
}
