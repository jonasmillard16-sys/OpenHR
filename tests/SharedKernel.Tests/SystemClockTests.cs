using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.SharedKernel.Tests;

public class SystemClockTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentTime()
    {
        var clock = new SystemClock();
        var before = DateTime.UtcNow;
        var result = clock.UtcNow;
        var after = DateTime.UtcNow;
        Assert.InRange(result, before, after);
    }

    [Fact]
    public void TodaySweden_ReturnsDateOnly()
    {
        var clock = new SystemClock();
        var today = clock.TodaySweden;
        // Swedish time is UTC+1 or UTC+2, so today should be within 1 day of UTC
        var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
        var diff = Math.Abs(today.DayNumber - utcToday.DayNumber);
        Assert.True(diff <= 1, $"Swedish date {today} should be within 1 day of UTC {utcToday}");
    }
}
