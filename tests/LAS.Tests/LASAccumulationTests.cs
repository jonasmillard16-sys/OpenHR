using RegionHR.LAS.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.LAS.Tests;

public class LASAccumulationTests
{
    [Fact]
    public void NyAckumulering_StartarPaNoll()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        Assert.Equal(0, acc.AckumuleradeDagar);
        Assert.Equal(LASStatus.UnderGrans, acc.Status);
    }

    [Fact]
    public void LaggTillPeriod_AckumulerarDagar()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var slut = start.AddDays(89); // 90 dagar

        acc.LaggTillPeriod(start, slut);

        Assert.Equal(90, acc.AckumuleradeDagar);
    }

    [Fact]
    public void SAVA_Over10Manader_GerAlarm()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-310));
        var slut = DateOnly.FromDateTime(DateTime.Today);

        acc.LaggTillPeriod(start, slut);

        Assert.Equal(LASStatus.NaraGrans, acc.Status);
    }

    [Fact]
    public void SAVA_Over11Manader_GerKritisktAlarm()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-340));
        var slut = DateOnly.FromDateTime(DateTime.Today);

        acc.LaggTillPeriod(start, slut);

        Assert.Equal(LASStatus.KritiskNara, acc.Status);
    }

    [Fact]
    public void SAVA_Over12Manader_TriggrarKonvertering()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-370));
        var slut = DateOnly.FromDateTime(DateTime.Today);

        acc.LaggTillPeriod(start, slut);

        Assert.Equal(LASStatus.KonverteradTillTillsvidare, acc.Status);
        Assert.NotNull(acc.KonverteringsDatum);
    }

    [Fact]
    public void Foretradesratt_SAVA_Beviljas_Over9Manader()
    {
        // SAVA: företrädesrätt efter 9 månader (~274 dagar) i en 3-årsperiod
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-290));
        var slut = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));

        acc.LaggTillPeriod(start, slut);
        acc.SattForetradesratt(slut);

        Assert.True(acc.HarForetradesratt);
        Assert.NotNull(acc.ForetradesrattUtgar);
    }

    [Fact]
    public void Foretradesratt_SAVA_Beviljas_Ej_Under9Manader()
    {
        // SAVA: under 9 månader (~274 dagar) → ingen företrädesrätt
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.SAVA);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-200));
        var slut = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));

        acc.LaggTillPeriod(start, slut);
        acc.SattForetradesratt(slut);

        Assert.False(acc.HarForetradesratt);
    }

    [Fact]
    public void EjLAS_ForTillsvidare_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.Tillsvidare));
    }

    [Fact]
    public void Vikariat_Over2Ar_TriggrarKonvertering()
    {
        var acc = LASAccumulation.Skapa(EmployeeId.New(), EmploymentType.Vikariat);
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-740));
        var slut = DateOnly.FromDateTime(DateTime.Today);

        acc.LaggTillPeriod(start, slut);

        Assert.Equal(LASStatus.KonverteradTillTillsvidare, acc.Status);
    }
}
