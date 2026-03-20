using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class WFMTests
{
    private readonly EmployeeId _anstallId = EmployeeId.New();
    private readonly OrganizationId _enhetId = OrganizationId.New();

    #region DemandForecast

    [Fact]
    public void DemandForecast_Skapa_SkaparMedKorrekVarden()
    {
        var forecast = DemandForecast.Skapa(
            _enhetId, new DateOnly(2026, 4, 1), 12, 96.0m, 85.5m);

        Assert.Equal(_enhetId, forecast.EnhetId);
        Assert.Equal(new DateOnly(2026, 4, 1), forecast.Datum);
        Assert.Equal(12, forecast.BeraknatAntal);
        Assert.Equal(96.0m, forecast.BeraknadeTidmmar);
        Assert.Equal(85.5m, forecast.Konfidensgrad);
        Assert.NotEqual(default, forecast.Id);
    }

    [Fact]
    public void DemandForecast_NegativtAntal_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            DemandForecast.Skapa(_enhetId, new DateOnly(2026, 4, 1), -1, 0m, 50m));
    }

    [Fact]
    public void DemandForecast_KonfidensgradOver100_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            DemandForecast.Skapa(_enhetId, new DateOnly(2026, 4, 1), 10, 80m, 101m));
    }

    [Fact]
    public void DemandForecast_KonfidensgradUnder0_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            DemandForecast.Skapa(_enhetId, new DateOnly(2026, 4, 1), 10, 80m, -1m));
    }

    #endregion

    #region ShiftCoverageRequest

    [Fact]
    public void ShiftCoverageRequest_Skapa_StarMedOpenStatus()
    {
        var request = ShiftCoverageRequest.Skapa(Guid.NewGuid(), "Sjuk medarbetare");

        Assert.Equal(CoverageStatus.Open, request.Status);
        Assert.Equal("Sjuk medarbetare", request.Anledning);
        Assert.Null(request.TilldeladAnstallId);
    }

    [Fact]
    public void ShiftCoverageRequest_Skapa_TomAnledning_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            ShiftCoverageRequest.Skapa(Guid.NewGuid(), ""));
    }

    [Fact]
    public void ShiftCoverageRequest_Tilldela_FlytasTillCovered()
    {
        var request = ShiftCoverageRequest.Skapa(Guid.NewGuid(), "Sjuk");
        var anstallId = EmployeeId.New();

        request.Tilldela(anstallId);

        Assert.Equal(CoverageStatus.Covered, request.Status);
        Assert.Equal(anstallId, request.TilldeladAnstallId);
    }

    [Fact]
    public void ShiftCoverageRequest_Tilldela_NarRedanCovered_KastarException()
    {
        var request = ShiftCoverageRequest.Skapa(Guid.NewGuid(), "Sjuk");
        request.Tilldela(EmployeeId.New());

        Assert.Throws<InvalidOperationException>(() =>
            request.Tilldela(EmployeeId.New()));
    }

    [Fact]
    public void ShiftCoverageRequest_MarkeraOtackt_FlytasTillUncovered()
    {
        var request = ShiftCoverageRequest.Skapa(Guid.NewGuid(), "Ingen tillganglig");

        request.MarkeraOtackt();

        Assert.Equal(CoverageStatus.Uncovered, request.Status);
    }

    [Fact]
    public void ShiftCoverageRequest_MarkeraOtackt_NarCovered_KastarException()
    {
        var request = ShiftCoverageRequest.Skapa(Guid.NewGuid(), "Sjuk");
        request.Tilldela(EmployeeId.New());

        Assert.Throws<InvalidOperationException>(() =>
            request.MarkeraOtackt());
    }

    #endregion

    #region FatigueScore

    [Fact]
    public void FatigueScore_Berakna_BasVarden_GerLagPoang()
    {
        var score = FatigueScore.Berakna(
            _anstallId, konsekutivaDagar: 3, nattpassSenaste7Dagar: 0,
            totalTimmarSenaste7Dagar: 30m, kortVila: 0, helgarbeteSenaste4Veckor: 0);

        Assert.Equal(0, score.Poang);
        Assert.Equal(_anstallId, score.AnstallId);
    }

    [Fact]
    public void FatigueScore_Berakna_HogBelastning_GerHogPoang()
    {
        var score = FatigueScore.Berakna(
            _anstallId, konsekutivaDagar: 7, nattpassSenaste7Dagar: 3,
            totalTimmarSenaste7Dagar: 48m, kortVila: 2, helgarbeteSenaste4Veckor: 4);

        // (7-4)*5=15 + 3*8=24 + (48-38.25)*2=19 + 2*10=20 + 4*3=12 = 90
        Assert.True(score.Poang > 50);
        Assert.True(score.Poang <= 100);
    }

    [Fact]
    public void FatigueScore_Berakna_MaxCap100()
    {
        var score = FatigueScore.Berakna(
            _anstallId, konsekutivaDagar: 14, nattpassSenaste7Dagar: 7,
            totalTimmarSenaste7Dagar: 80m, kortVila: 5, helgarbeteSenaste4Veckor: 8);

        Assert.Equal(100, score.Poang);
    }

    [Fact]
    public void FatigueScore_Berakna_SattKorrektatributvarden()
    {
        var score = FatigueScore.Berakna(
            _anstallId, konsekutivaDagar: 5, nattpassSenaste7Dagar: 2,
            totalTimmarSenaste7Dagar: 42m, kortVila: 1, helgarbeteSenaste4Veckor: 3);

        Assert.Equal(5, score.KonsekutivaDagar);
        Assert.Equal(2, score.NattpassSenaste7Dagar);
        Assert.Equal(42m, score.TotalTimmarSenaste7Dagar);
        Assert.Equal(1, score.KortVila);
        Assert.Equal(3, score.HelgarbeteSenaste4Veckor);
        Assert.NotEqual(default, score.BeraknadVid);
    }

    #endregion

    #region SchedulingRun

    [Fact]
    public void SchedulingRun_Starta_StartarMedRunningStatus()
    {
        var run = SchedulingRun.Starta(
            _enhetId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 7), "{}");

        Assert.Equal("Running", run.Status);
        Assert.Equal(_enhetId, run.EnhetId);
        Assert.Equal(0, run.GenereradePass);
    }

    [Fact]
    public void SchedulingRun_Slutfor_SatterCompleteOchVarden()
    {
        var run = SchedulingRun.Starta(
            _enhetId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 7), "{}");

        run.Slutfor(42, 12500m, 3200m, true);

        Assert.Equal("Complete", run.Status);
        Assert.Equal(42, run.GenereradePass);
        Assert.Equal(12500m, run.TotalOBKostnad);
        Assert.Equal(3200m, run.TotalOvertidKostnad);
        Assert.True(run.ATLKompliant);
    }

    [Fact]
    public void SchedulingRun_MarkFailed_SatterFailedStatus()
    {
        var run = SchedulingRun.Starta(
            _enhetId, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 7), "{}");

        run.MarkFailed();

        Assert.Equal("Failed", run.Status);
    }

    #endregion
}
