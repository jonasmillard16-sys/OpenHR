using Xunit;
using RegionHR.Compensation.Domain;

namespace RegionHR.Compensation.Tests;

public class CompensationBandTests
{
    private static CompensationBand SkapaBand() => new()
    {
        Befattningskategori = "Sjukskoterska",
        Min = 32000m, Mal = 38500m, Max = 45000m
    };

    [Fact]
    public void ArInomBand_med_lon_inom_intervall_returnerar_true()
    {
        var band = SkapaBand();
        Assert.True(band.ArInomBand(38500m));
    }

    [Fact]
    public void ArInomBand_med_lon_under_min_returnerar_false()
    {
        var band = SkapaBand();
        Assert.False(band.ArInomBand(31000m));
    }

    [Fact]
    public void ArInomBand_med_lon_over_max_returnerar_false()
    {
        var band = SkapaBand();
        Assert.False(band.ArInomBand(46000m));
    }

    [Fact]
    public void ArInomBand_pa_min_gransvarde_returnerar_true()
    {
        var band = SkapaBand();
        Assert.True(band.ArInomBand(32000m));
    }

    [Fact]
    public void ArInomBand_pa_max_gransvarde_returnerar_true()
    {
        var band = SkapaBand();
        Assert.True(band.ArInomBand(45000m));
    }

    [Fact]
    public void BandPosition_beraknar_korrekt_mittenprocent()
    {
        var band = SkapaBand();
        // 38500 ar mitt i 32000-45000 = (38500-32000)/(45000-32000)*100 = 50%
        var pos = band.BandPosition(38500m);
        Assert.Equal(50.0m, Math.Round(pos, 1));
    }

    [Fact]
    public void BandPosition_pa_min_ger_noll()
    {
        var band = SkapaBand();
        Assert.Equal(0m, band.BandPosition(32000m));
    }

    [Fact]
    public void BandPosition_pa_max_ger_hundra()
    {
        var band = SkapaBand();
        Assert.Equal(100m, band.BandPosition(45000m));
    }

    [Fact]
    public void BandPosition_over_max_clampas_till_hundra()
    {
        var band = SkapaBand();
        Assert.Equal(100m, band.BandPosition(50000m));
    }
}
