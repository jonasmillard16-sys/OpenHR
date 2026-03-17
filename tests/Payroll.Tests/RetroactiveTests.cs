using Xunit;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Tests;

/// <summary>
/// Tester för retroaktiv löneomräkning.
/// </summary>
public class RetroactiveTests
{
    private readonly RetroactiveRecalculationEngine _engine;
    private readonly MockTaxTableProvider _taxProvider;

    public RetroactiveTests()
    {
        _taxProvider = new MockTaxTableProvider();
        _engine = new RetroactiveRecalculationEngine(_taxProvider);
    }

    private static PayrollResult CreateResult(
        decimal manadslon,
        decimal sysselsattningsgrad,
        int year,
        int month,
        decimal? obBelopp = null,
        decimal? overtidBelopp = null,
        decimal? jourBelopp = null,
        decimal? beredskapsBelopp = null,
        decimal skatt = 8000m,
        decimal agAvgifter = 11000m,
        decimal pension = 2100m)
    {
        var runId = PayrollRunId.New();
        var employeeId = EmployeeId.New();
        var employmentId = EmploymentId.New();

        var result = PayrollResult.Skapa(
            runId, employeeId, employmentId,
            year, month, Money.SEK(manadslon),
            sysselsattningsgrad, CollectiveAgreementType.AB);

        // Grundlön
        var grundlon = manadslon * sysselsattningsgrad / 100m;
        result.LaggTillRad(new PayrollResultLine
        {
            LoneartKod = "1100", Benamning = "Månadslön",
            Antal = 1, Sats = Money.SEK(manadslon),
            Belopp = Money.SEK(grundlon),
            Skattekategori = TaxCategory.Skattepliktig,
            ArSemestergrundande = true, ArPensionsgrundande = true
        });

        var brutto = grundlon;

        // OB
        if (obBelopp.HasValue && obBelopp.Value != 0)
        {
            result.LaggTillRad(new PayrollResultLine
            {
                LoneartKod = "1310", Benamning = "OB-tillägg",
                Antal = 1, Sats = Money.SEK(obBelopp.Value),
                Belopp = Money.SEK(obBelopp.Value),
                Skattekategori = TaxCategory.Skattepliktig,
                ArSemestergrundande = true, ArPensionsgrundande = true
            });
            result.OBTillagg = Money.SEK(obBelopp.Value);
            brutto += obBelopp.Value;
        }

        // Övertid
        if (overtidBelopp.HasValue && overtidBelopp.Value != 0)
        {
            result.LaggTillRad(new PayrollResultLine
            {
                LoneartKod = "1410", Benamning = "Enkel övertid",
                Antal = 1, Sats = Money.SEK(overtidBelopp.Value),
                Belopp = Money.SEK(overtidBelopp.Value),
                Skattekategori = TaxCategory.Skattepliktig,
                ArSemestergrundande = false, ArPensionsgrundande = true
            });
            result.Overtidstillagg = Money.SEK(overtidBelopp.Value);
            brutto += overtidBelopp.Value;
        }

        // Jour
        if (jourBelopp.HasValue && jourBelopp.Value != 0)
        {
            result.LaggTillRad(new PayrollResultLine
            {
                LoneartKod = "1500", Benamning = "Jour",
                Antal = 1, Sats = Money.SEK(jourBelopp.Value),
                Belopp = Money.SEK(jourBelopp.Value),
                Skattekategori = TaxCategory.Skattepliktig,
                ArSemestergrundande = true, ArPensionsgrundande = true
            });
            result.JourTillagg = Money.SEK(jourBelopp.Value);
            brutto += jourBelopp.Value;
        }

        // Beredskap
        if (beredskapsBelopp.HasValue && beredskapsBelopp.Value != 0)
        {
            result.LaggTillRad(new PayrollResultLine
            {
                LoneartKod = "1510", Benamning = "Beredskap",
                Antal = 1, Sats = Money.SEK(beredskapsBelopp.Value),
                Belopp = Money.SEK(beredskapsBelopp.Value),
                Skattekategori = TaxCategory.Skattepliktig,
                ArSemestergrundande = true, ArPensionsgrundande = true
            });
            result.BeredskapsTillagg = Money.SEK(beredskapsBelopp.Value);
            brutto += beredskapsBelopp.Value;
        }

        result.Brutto = Money.SEK(brutto);
        result.SkattepliktBrutto = Money.SEK(brutto);
        result.Skatt = Money.SEK(skatt);
        result.Netto = Money.SEK(brutto - skatt);
        result.Arbetsgivaravgifter = Money.SEK(agAvgifter);
        result.Pensionsavgift = Money.SEK(pension);

        return result;
    }

    #region Retroaktiv med löneökning

    [Fact]
    public async Task Recalculate_LoneOkning_CorrectDifference()
    {
        // Original: 35 000 kr/mån
        var original = CreateResult(35000m, 100m, 2025, 1,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);

        // Ny: 37 000 kr/mån (löneökning)
        var recalculated = CreateResult(37000m, 100m, 2025, 1,
            skatt: 8500m, agAvgifter: 11630m, pension: 2220m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        Assert.Equal("2025-01", result.OriginalPeriod);

        // Bruttodifferens = 37000 - 35000 = 2000
        Assert.Equal(2000m, result.BruttoDifferens.Amount);

        // Skattedifferens = 8500 - 8000 = 500
        Assert.Equal(500m, result.SkatteDifferens.Amount);

        // Nettodifferens = 2000 - 500 = 1500
        Assert.Equal(1500m, result.NettoDifferens.Amount);

        // Ska ha en grundlönedifferensrad
        var grundlonDiff = result.DifferenceLines.FirstOrDefault(d => d.LoneartKod == "7100");
        Assert.NotNull(grundlonDiff);
        Assert.Equal(2000m, grundlonDiff.Differens.Amount);
        Assert.Equal(35000m, grundlonDiff.OriginalBelopp.Amount);
        Assert.Equal(37000m, grundlonDiff.NyttBelopp.Amount);
    }

    #endregion

    #region Retroaktiv med ändrad sysselsättningsgrad

    [Fact]
    public async Task Recalculate_AndradSysselsattningsgrad_CorrectDifference()
    {
        // Original: 35 000 kr/mån, 75% sysselsättning = 26250
        var original = CreateResult(35000m, 75m, 2025, 3,
            skatt: 5000m, agAvgifter: 8250m, pension: 1575m);

        // Ny: 35 000 kr/mån, 100% sysselsättning = 35000
        var recalculated = CreateResult(35000m, 100m, 2025, 3,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        // Bruttodifferens = 35000 - 26250 = 8750
        Assert.Equal(8750m, result.BruttoDifferens.Amount);

        // Skattedifferens = 8000 - 5000 = 3000
        Assert.Equal(3000m, result.SkatteDifferens.Amount);

        // Nettodifferens = 8750 - 3000 = 5750
        Assert.Equal(5750m, result.NettoDifferens.Amount);

        // Arbetsgivaravgiftdifferens
        Assert.Equal(2750m, result.ArbetsgivaravgiftDifferens.Amount);

        // Pensionsdifferens
        Assert.Equal(525m, result.PensionDifferens.Amount);
    }

    #endregion

    #region Retroaktiv över årsgräns

    [Fact]
    public async Task Recalculate_OverArsgrans_SchablonskattPa30Procent()
    {
        // Original: december 2024, 35 000 kr
        var original = CreateResult(35000m, 100m, 2024, 12,
            skatt: 7500m, agAvgifter: 11000m, pension: 2100m);

        // Ny: december 2024 med ny lön 37 000 kr, men betalas ut 2025
        var recalculated = CreateResult(37000m, 100m, 2024, 12,
            skatt: 8000m, agAvgifter: 11630m, pension: 2220m);

        // taxTableYear = 2025 (annat år än original 2024)
        var result = await _engine.RecalculateAsync(original, recalculated, taxTableYear: 2025);

        // Bruttodifferens = 37000 - 35000 = 2000
        Assert.Equal(2000m, result.BruttoDifferens.Amount);

        // Vid årsövergång: retroskatt = bruttodiff * 30% = 2000 * 0.30 = 600 (avrundat till kronor)
        // Skattedifferens = 600 (schablon istället för tabellberäkning)
        Assert.Equal(600m, result.SkatteDifferens.Amount);

        // Nettodifferens = 2000 - 600 = 1400
        Assert.Equal(1400m, result.NettoDifferens.Amount);
    }

    [Fact]
    public async Task Recalculate_SammaAr_IngenSchablonskatt()
    {
        // Original och nytt inom samma år
        var original = CreateResult(35000m, 100m, 2025, 1,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);
        var recalculated = CreateResult(37000m, 100m, 2025, 1,
            skatt: 8500m, agAvgifter: 11630m, pension: 2220m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        // Utan årsövergång: normal skattedifferens
        Assert.Equal(500m, result.SkatteDifferens.Amount);
    }

    #endregion

    #region Nettodifferens korrekt beräknad

    [Fact]
    public async Task Recalculate_NettoDifferens_AlltidKorrekt()
    {
        var original = CreateResult(35000m, 100m, 2025, 6,
            obBelopp: 1265m,
            skatt: 8500m, agAvgifter: 11400m, pension: 2175m);

        var recalculated = CreateResult(37000m, 100m, 2025, 6,
            obBelopp: 1265m,
            skatt: 9000m, agAvgifter: 12020m, pension: 2295m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        // Nettodifferens = bruttodiff - skattediff
        var expectedNetto = result.BruttoDifferens - result.SkatteDifferens;
        Assert.Equal(expectedNetto, result.NettoDifferens);
    }

    [Fact]
    public async Task Recalculate_IngenAndring_TomtResultat()
    {
        var original = CreateResult(35000m, 100m, 2025, 4,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);

        // Samma resultat
        var recalculated = CreateResult(35000m, 100m, 2025, 4,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        Assert.Equal(Money.Zero, result.BruttoDifferens);
        Assert.Equal(Money.Zero, result.NettoDifferens);
        Assert.Empty(result.DifferenceLines);
    }

    [Fact]
    public async Task Recalculate_LoneSankning_NegativDifferens()
    {
        var original = CreateResult(37000m, 100m, 2025, 5,
            skatt: 8500m, agAvgifter: 11630m, pension: 2220m);

        var recalculated = CreateResult(35000m, 100m, 2025, 5,
            skatt: 8000m, agAvgifter: 11000m, pension: 2100m);

        var result = await _engine.RecalculateAsync(original, recalculated);

        // Negativ differens = lönesänkning
        Assert.True(result.BruttoDifferens < Money.Zero);
        Assert.True(result.NettoDifferens < Money.Zero);

        var grundlonDiff = result.DifferenceLines.First(d => d.LoneartKod == "7100");
        Assert.True(grundlonDiff.ArAvdrag);
    }

    #endregion

    #region Mock

    private sealed class MockTaxTableProvider : ITaxTableProvider
    {
        public Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default)
        {
            var table = new TaxTable { Id = 1, Ar = year, Tabellnummer = tableNumber, Kolumn = column };
            table.LaggTillRad(new TaxTableRow { InkomstFran = 0m, InkomstTill = 19999m, Skattebelopp = 0m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 20000m, InkomstTill = 29999m, Skattebelopp = 5000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 30000m, InkomstTill = 39999m, Skattebelopp = 8000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 40000m, InkomstTill = 999999m, Skattebelopp = 12000m });
            return Task.FromResult<TaxTable?>(table);
        }

        public Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<TaxTable>>(new List<TaxTable>());
        }
    }

    #endregion
}
