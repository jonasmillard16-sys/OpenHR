using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.SharedKernel.Tests;

public class MoneyTests
{
    [Fact]
    public void Addition_SammaValuta_Summerar()
    {
        var a = Money.SEK(100m);
        var b = Money.SEK(50.50m);
        var result = a + b;
        Assert.Equal(150.50m, result.Amount);
    }

    [Fact]
    public void Subtraktion_GerKorrektResultat()
    {
        var brutto = Money.SEK(35000m);
        var skatt = Money.SEK(8750m);
        var netto = brutto - skatt;
        Assert.Equal(26250m, netto.Amount);
    }

    [Fact]
    public void Multiplikation_MedFaktor()
    {
        var lon = Money.SEK(35000m);
        var avgift = lon * 0.3142m;
        Assert.Equal(10997m, avgift.Amount);
    }

    [Fact]
    public void RoundToOren_AvrundarTillTvaDecimaler()
    {
        var belopp = Money.SEK(1234.5678m);
        var avrundat = belopp.RoundToOren();
        Assert.Equal(1234.57m, avrundat.Amount);
    }

    [Fact]
    public void RoundToKronor_AvrundarTillHeltal()
    {
        var belopp = Money.SEK(1234.56m);
        var avrundat = belopp.RoundToKronor();
        Assert.Equal(1235m, avrundat.Amount);
    }

    [Fact]
    public void OlikaValutor_KastarException()
    {
        var sek = Money.SEK(100m);
        var eur = new Money(100m, "EUR");
        Assert.Throws<InvalidOperationException>(() => sek + eur);
    }

    [Fact]
    public void Zero_ArNoll()
    {
        Assert.Equal(0m, Money.Zero.Amount);
    }

    [Fact]
    public void Jamforelse_FungerarKorrekt()
    {
        var a = Money.SEK(100m);
        var b = Money.SEK(200m);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= b);
    }
}
