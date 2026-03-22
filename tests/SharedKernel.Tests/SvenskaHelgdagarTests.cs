using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.SharedKernel.Tests;

public class SvenskaHelgdagarTests
{
    [Theory]
    [InlineData(2026, 1, 1, true)]    // Nyårsdagen
    [InlineData(2026, 1, 6, true)]    // Trettondedag jul
    [InlineData(2026, 5, 1, true)]    // Första maj
    [InlineData(2026, 6, 6, true)]    // Nationaldag
    [InlineData(2026, 12, 25, true)]  // Juldagen
    [InlineData(2026, 12, 26, true)]  // Annandag jul
    [InlineData(2026, 3, 23, false)]  // Vanlig måndag
    [InlineData(2026, 9, 15, false)]  // Vanlig tisdag
    public void ArHelgdag_FixedHolidays(int y, int m, int d, bool expected)
    {
        Assert.Equal(expected, SvenskaHelgdagar.ArHelgdag(new DateOnly(y, m, d)));
    }

    [Fact]
    public void ArHelgdag_Easter2026_April5()
    {
        // Easter 2026 is April 5
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 4, 3)));  // Långfredagen
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 4, 4)));  // Påskafton
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 4, 5)));  // Påskdagen
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 4, 6)));  // Annandag påsk
    }

    [Fact]
    public void ArHelgdag_KristiHimmelsfardsdag2026()
    {
        // Easter 2026 = April 5, +39 days = May 14
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 5, 14)));
    }

    [Fact]
    public void ArHelgdag_Pingstdagen2026()
    {
        // Easter 2026 = April 5, +49 days = May 24
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 5, 24)));
    }

    [Fact]
    public void ArHelgdag_Midsommar2026()
    {
        // 2026: Jun 19 is a Friday → midsommarafton = Jun 19, midsommardagen = Jun 20
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 6, 19)));  // Afton
        Assert.True(SvenskaHelgdagar.ArHelgdag(new DateOnly(2026, 6, 20)));  // Dagen
    }

    [Fact]
    public void HelgdagarForAr_ContainsAtLeast13()
    {
        var holidays = SvenskaHelgdagar.HelgdagarForAr(2026);
        Assert.True(holidays.Count >= 13);
    }
}
