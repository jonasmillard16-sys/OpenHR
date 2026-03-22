using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.SharedKernel.Tests;

public class DateRangeHolidayTests
{
    [Fact]
    public void WorkDays_ExcludesChristmasHolidays()
    {
        // Dec 22 (Mon) to Dec 26 (Fri) 2025
        // Mon 22 = work, Tue 23 = work, Wed 24 = holiday (julafton), Thu 25 = holiday (juldagen), Fri 26 = holiday (annandag jul)
        var range = new DateRange(new DateOnly(2025, 12, 22), new DateOnly(2025, 12, 26));
        Assert.Equal(2, range.WorkDays);
    }

    [Fact]
    public void WorkDays_NormalWeek_Returns5()
    {
        // Mar 16-20, 2026 (Mon-Fri, no holidays)
        var range = new DateRange(new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 20));
        Assert.Equal(5, range.WorkDays);
    }

    [Fact]
    public void WorkDays_ExcludesMidsommar()
    {
        // Jun 15-19, 2026 (Mon-Fri). Jun 19 = midsommarafton (holiday)
        var range = new DateRange(new DateOnly(2026, 6, 15), new DateOnly(2026, 6, 19));
        Assert.Equal(4, range.WorkDays);
    }

    [Fact]
    public void WorkDays_CrossYearBoundary()
    {
        // Dec 29, 2025 (Mon) to Jan 2, 2026 (Fri)
        // Mon 29 = work, Tue 30 = work, Wed 31 = holiday (nyårsafton), Thu 1 = holiday (nyårsdagen), Fri 2 = work
        var range = new DateRange(new DateOnly(2025, 12, 29), new DateOnly(2026, 1, 2));
        Assert.Equal(3, range.WorkDays);
    }
}
