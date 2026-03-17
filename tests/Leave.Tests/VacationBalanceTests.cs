using RegionHR.Leave.Domain;
using Xunit;

namespace RegionHR.Leave.Tests;

public class VacationBalanceTests
{
    private readonly Guid _anstallId = Guid.NewGuid();

    [Theory]
    [InlineData(25, 25)]  // Under 40: 25 dagar
    [InlineData(30, 25)]
    [InlineData(39, 25)]
    [InlineData(40, 31)]  // 40-49: 31 dagar
    [InlineData(45, 31)]
    [InlineData(49, 31)]
    [InlineData(50, 32)]  // 50+: 32 dagar
    [InlineData(60, 32)]
    [InlineData(65, 32)]
    public void SkapaForAr_TilldelarKorrektAntalDagarBasertPaAlder(int alder, int forvantadTilldelning)
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, alder);

        Assert.Equal(forvantadTilldelning, saldo.Tilldelning);
    }

    [Fact]
    public void RegistreraUttag_MinskarTillgangligaDagar()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35); // 25 dagar

        saldo.RegistreraUttag(5);

        Assert.Equal(5, saldo.UttagnaDagar);
        Assert.Equal(20, saldo.TillgangligaDagar);
    }

    [Fact]
    public void RegistreraUttag_KastarNarDagarIntaRacker()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35); // 25 dagar

        Assert.Throws<InvalidOperationException>(() => saldo.RegistreraUttag(26));
    }

    [Fact]
    public void SparaDagar_LaggerTillSparadeDagar()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35); // 25 dagar, max sparade = 125

        saldo.SparaDagar(10);

        Assert.Equal(10, saldo.SparadeDagar);
    }

    [Fact]
    public void SparaDagar_KastarNarMaxGransenNas()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35); // 25 dagar, max sparade = 125

        saldo.SparaDagar(125);

        Assert.Throws<InvalidOperationException>(() => saldo.SparaDagar(1));
    }

    [Fact]
    public void TillgangligaDagar_InkluderarSparadeDagar()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 45); // 31 dagar

        saldo.SparaDagar(10);

        // Tillgngliga = Tilldelning(31) + Sparade(10) - Uttagna(0) = 41
        Assert.Equal(41, saldo.TillgangligaDagar);
    }

    [Fact]
    public void TillgangligaDagar_BerknasKorrekt()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 50); // 32 dagar

        saldo.SparaDagar(5);
        saldo.RegistreraUttag(10);

        // Tillgngliga = Tilldelning(32) + Sparade(5) - Uttagna(10) = 27
        Assert.Equal(27, saldo.TillgangligaDagar);
    }

    [Fact]
    public void RegistreraUttag_KastarVidNollEllerNegativt()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35);

        Assert.Throws<ArgumentOutOfRangeException>(() => saldo.RegistreraUttag(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => saldo.RegistreraUttag(-1));
    }

    [Fact]
    public void SparaDagar_KastarVidNollEllerNegativt()
    {
        var saldo = VacationBalance.SkapaForAr(_anstallId, 2026, 35);

        Assert.Throws<ArgumentOutOfRangeException>(() => saldo.SparaDagar(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => saldo.SparaDagar(-1));
    }
}
