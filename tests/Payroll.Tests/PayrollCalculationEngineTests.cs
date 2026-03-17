using Xunit;
using RegionHR.Core.Contracts;
using RegionHR.Payroll.Domain;
using RegionHR.Payroll.Engine;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Tests;

/// <summary>
/// Enhetstester för löneberäkningsmotorn.
/// Testar hela brutto-till-netto-pipelinen med svenska skatteregler och kollektivavtal.
/// </summary>
public class PayrollCalculationEngineTests
{
    private readonly PayrollCalculationEngine _engine;
    private readonly MockTaxTableProvider _taxProvider;
    private readonly MockCollectiveAgreementRulesEngine _rulesEngine;
    private readonly MockCoreHRModule _coreHR;

    // Standard testmedarbetare
    private readonly EmployeeId _employeeId = EmployeeId.New();
    private readonly EmploymentId _employmentId = EmploymentId.New();
    private readonly PayrollRunId _runId = PayrollRunId.New();
    private readonly OrganizationId _unitId = OrganizationId.New();

    public PayrollCalculationEngineTests()
    {
        _taxProvider = new MockTaxTableProvider();
        _rulesEngine = new MockCollectiveAgreementRulesEngine();
        _coreHR = new MockCoreHRModule();
        _engine = new PayrollCalculationEngine(_taxProvider, _rulesEngine, _coreHR);
    }

    private void SetupStandardEmployee(
        decimal manadslon = 35000m,
        decimal sysselsattningsgrad = 100m,
        string personnummerMaskerat = "19850515-****",
        int? skattetabell = 33,
        int? skattekolumn = 1,
        bool harJamkning = false,
        decimal? jamkningBelopp = null)
    {
        _coreHR.Employee = new EmployeeDto(
            _employeeId, "Test", "Testsson", personnummerMaskerat, "test@test.se",
            skattetabell, skattekolumn, "Göteborg", 32.00m, false, null,
            harJamkning, jamkningBelopp);

        _coreHR.Employment = new EmploymentDto(
            _employmentId, _employeeId, _unitId,
            EmploymentType.Tillsvidare, CollectiveAgreementType.AB,
            manadslon, sysselsattningsgrad,
            new DateOnly(2020, 1, 1), null, "201100");
    }

    #region Grundlön

    [Fact]
    public async Task CalculateAsync_FullManad_ReturnsCorrectGrundlon()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            Kostnadsstalle = "1234"
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Full månadslön = 35000
        var grundlonRad = result.Rader.First(r => r.LoneartKod == "1100");
        Assert.Equal(35000m, grundlonRad.Belopp.Amount);
    }

    [Fact]
    public async Task CalculateAsync_DelAvManad_ProportionalGrundlon()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 10,
            ArbetsdagarIManadens = 21,
            Kostnadsstalle = "1234"
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Proportionell: 35000 * 10/21
        var expected = Math.Round(35000m * 10m / 21m, 2);
        var grundlonRad = result.Rader.First(r => r.LoneartKod == "1100");
        Assert.Equal(expected, Math.Round(grundlonRad.Belopp.Amount, 2));
    }

    [Fact]
    public async Task CalculateAsync_Deltid75Procent_CorrectGrundlon()
    {
        SetupStandardEmployee(manadslon: 35000m, sysselsattningsgrad: 75m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Grundlön = 35000 * 75/100 = 26250
        var grundlonRad = result.Rader.First(r => r.LoneartKod == "1100");
        Assert.Equal(26250m, grundlonRad.Belopp.Amount);
    }

    #endregion

    #region OB-tillägg

    [Fact]
    public async Task CalculateAsync_MedOBTimmar_ReturnsCorrectOBTillagg()
    {
        SetupStandardEmployee();
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OBTimmar =
            [
                new OBInput { Kategori = OBCategory.VardagKvall, Timmar = 10 },
                new OBInput { Kategori = OBCategory.VardagNatt, Timmar = 5 },
                new OBInput { Kategori = OBCategory.Storhelg, Timmar = 8 }
            ]
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // AB satser: VardagKvall=126.50, VardagNatt=152.00, Storhelg=195.00
        var expectedOB = 10 * 126.50m + 5 * 152.00m + 8 * 195.00m;
        Assert.Equal(expectedOB, result.OBTillagg.Amount);

        var obRader = result.Rader.Where(r => r.LoneartKod == "1310").ToList();
        Assert.Equal(3, obRader.Count);
    }

    [Theory]
    [InlineData(OBCategory.VardagKvall, 126.50)]
    [InlineData(OBCategory.VardagNatt, 152.00)]
    [InlineData(OBCategory.Helg, 89.00)]
    [InlineData(OBCategory.Storhelg, 195.00)]
    public async Task CalculateAsync_OBPerKategori_CorrectRate(OBCategory kategori, decimal expectedRate)
    {
        SetupStandardEmployee();
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OBTimmar = [new OBInput { Kategori = kategori, Timmar = 10 }]
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        Assert.Equal(10 * expectedRate, result.OBTillagg.Amount);
    }

    #endregion

    #region Övertid

    [Fact]
    public async Task CalculateAsync_EnkelOvertid_CorrectBerakna()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OvertidTimmar = 10,
            KvalificeradOvertid = false
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Timlön = 35000 / (38.25 * 52 / 12) = 35000 / 165.75
        // AB 25: Enkel övertid tillägg = 0.8x timlön (180% total minus 100% bas)
        var timlon = 35000m / (38.25m * 52m / 12m);
        var expected = 10 * timlon * 0.8m;
        Assert.Equal(expected, result.Overtidstillagg.Amount);

        var overtidRad = result.Rader.First(r => r.LoneartKod == "1410");
        Assert.Equal("Enkel övertid", overtidRad.Benamning);
    }

    [Fact]
    public async Task CalculateAsync_KvalificeradOvertid_CorrectBerakna()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OvertidTimmar = 5,
            KvalificeradOvertid = true
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // AB 25: Kvalificerad övertid tillägg = 1.4x timlön (240% total minus 100% bas)
        var timlon = 35000m / (38.25m * 52m / 12m);
        var expected = 5 * timlon * 1.4m;
        Assert.Equal(expected, result.Overtidstillagg.Amount);

        var overtidRad = result.Rader.First(r => r.LoneartKod == "1420");
        Assert.Equal("Kvalificerad övertid", overtidRad.Benamning);
    }

    #endregion

    #region Jour och beredskap

    [Fact]
    public async Task CalculateAsync_MedJourTimmar_CorrectJourTillagg()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            JourTimmar = 16
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Jour = timlön * 0.40 * 16
        // Motorn beräknar: jourSats = timlon * 0.40, sedan jourTillagg = timmar * jourSats
        var timlon = 35000m / (38.25m * 52m / 12m);
        var jourSats = timlon * 0.40m;
        var expected = 16 * jourSats;
        Assert.Equal(expected, result.JourTillagg.Amount);

        var jourRad = result.Rader.First(r => r.LoneartKod == "1500");
        Assert.Equal("Jour", jourRad.Benamning);
        Assert.True(jourRad.ArSemestergrundande);
        Assert.True(jourRad.ArPensionsgrundande);
    }

    [Fact]
    public async Task CalculateAsync_MedBeredskapsTimmar_CorrectBeredskapsTillagg()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            BeredskapsTimmar = 24
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Beredskap = timlön * 0.20 * 24
        var timlon = 35000m / (38.25m * 52m / 12m);
        var expected = 24 * timlon * 0.20m;
        Assert.Equal(expected, result.BeredskapsTillagg.Amount);

        var beredskapsRad = result.Rader.First(r => r.LoneartKod == "1510");
        Assert.Equal("Beredskap", beredskapsRad.Benamning);
    }

    #endregion

    #region Sjuklön och karensavdrag

    [Fact]
    public async Task CalculateAsync_MedSjukdagar_CorrectSjuklonOchKarens()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            SjukdagarMedLon = 5
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Karensavdrag = veckolön * 80% * 20% = (35000 * 12 / 52) * 0.80 * 0.20
        // Per Sjuklönelagen 6§: 20% av genomsnittlig veckosjuklön (som är 80% av veckolön)
        var veckolon = 35000m * 100m / 100m * 12m / 52m;
        var expectedKarens = veckolon * 0.80m * 0.20m;
        Assert.Equal(expectedKarens, result.Karensavdrag.Amount);

        // Sjuklön = dagslön * 80% * 5 dagar
        var daglon = 35000m * 100m / 100m / 21m;
        var expectedSjuklon = daglon * 0.80m * 5;
        Assert.Equal(expectedSjuklon, result.Sjuklon.Amount);

        // Karensavdrag ska vara negativt (avdrag) på raden
        var karensRad = result.Rader.First(r => r.LoneartKod == "3001");
        Assert.True(karensRad.ArAvdrag);
        Assert.True(karensRad.Belopp.Amount < 0);
    }

    #endregion

    #region Semester

    [Fact]
    public async Task CalculateAsync_MedSemester_CorrectSemesterAvdragOchTillagg()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            SemesterdagarUttagna = 5
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Semesteravdrag = 35000 * 100/100 * 0.80% * 5 = 1400
        var semesterAvdrag = 35000m * 0.0080m * 5;
        var avdragRad = result.Rader.First(r => r.LoneartKod == "2710");
        Assert.Equal(-semesterAvdrag, avdragRad.Belopp.Amount);
        Assert.True(avdragRad.ArAvdrag);

        // Semesterlön (sammalöneregeln) = samma belopp som avdraget
        var semesterlon = result.Rader.First(r => r.LoneartKod == "2700");
        Assert.Equal(semesterAvdrag, semesterlon.Belopp.Amount);

        // Semestertillägg = 35000 * 100/100 * 0.43% * 5 (AB 25)
        var expectedTillagg = 35000m * 100m / 100m * 0.0043m * 5;
        Assert.Equal(expectedTillagg, result.Semestertillagg.Amount);
        Assert.Equal(5, result.SemesterdagarUttagna);
    }

    #endregion

    #region Arbetsgivaravgifter

    [Theory]
    [InlineData("20030101-****", 2025, 0.2081)]  // Ung (22 under 2025, 19-22 → reducerad)
    [InlineData("19570101-****", 2025, 0.1021)]  // Äldre (68 under 2025, born 1957 → reducerad)
    [InlineData("19850515-****", 2025, 0.3142)]  // Standard (40 år)
    [InlineData("20020101-****", 2025, 0.3142)]  // Born 2002: 23 under 2025, utanför 19-22 → standard
    public async Task CalculateAsync_Arbetsgivaravgifter_CorrectSatsBasedOnAge(
        string personnummer, int year, decimal expectedSats)
    {
        SetupStandardEmployee(manadslon: 35000m, personnummerMaskerat: personnummer);

        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, year, 1, input);

        Assert.Equal(expectedSats, result.ArbetsgivaravgiftSats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_UngFodd2003_Reducerad2025()
    {
        // Born 2003 → 22 under 2025 → within 19-22 range → reduced
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(2003, 2025);
        Assert.Equal(0.2081m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_AldreFodd1957_Reducerad2025()
    {
        // 2025: Born 1957 → 68 under 2025 → reducerad (68+ threshold)
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(1957, 2025);
        Assert.Equal(0.1021m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_AldreFodd1959_Standard2025()
    {
        // 2025: Born 1959 → 66 under 2025 → NOT 68+ → standard rate
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(1959, 2025);
        Assert.Equal(0.3142m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_AldreFodd1959_Reducerad2026()
    {
        // 2026: Born 1959 → 67 under 2026 → 67+ threshold lowered → reducerad
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(1959, 2026);
        Assert.Equal(0.1021m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_AldreFodd1960_Standard2026()
    {
        // 2026: Born 1960 → 66 under 2026 → NOT 67+ → standard
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(1960, 2026);
        Assert.Equal(0.3142m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_UngFodd2004_Reducerad2026()
    {
        // 2026: Born 2004 → 22 under 2026 → within 19-22 → reduced
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(2004, 2026);
        Assert.Equal(0.2081m, sats);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftSats_StandardFodd1985_FullAvgift()
    {
        var sats = PayrollCalculationEngine.BeraknaArbetsgivaravgiftSatsFranFodelseAr(1985, 2025);
        Assert.Equal(0.3142m, sats); // 40 år
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftBelopp_UngSplitRate_CorrectBelopp()
    {
        // Born 2003, year 2025: youth rate on first 25000, standard on rest
        var brutto = Money.SEK(35000m);
        var belopp = PayrollCalculationEngine.BeraknaArbetsgivaravgiftBeloppFranFodelseAr(brutto, 2003, 2025);
        // 25000 * 0.2081 + 10000 * 0.3142
        var expected = 25000m * 0.2081m + 10000m * 0.3142m;
        Assert.Equal(expected, belopp.Amount);
    }

    [Fact]
    public void BeraknaArbetsgivaravgiftBelopp_UngUnderTak_OnlyReducedRate()
    {
        // Born 2003, year 2025: salary below 25000 → all at reduced rate
        var brutto = Money.SEK(20000m);
        var belopp = PayrollCalculationEngine.BeraknaArbetsgivaravgiftBeloppFranFodelseAr(brutto, 2003, 2025);
        var expected = 20000m * 0.2081m;
        Assert.Equal(expected, belopp.Amount);
    }

    #endregion

    #region AKAP-KR Pension

    [Fact]
    public async Task CalculateAsync_PensionUnder7_5IBB_6Procent()
    {
        // 35000 * 12 = 420 000 < 7.5 * 80 600 = 604 500
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Pensionsgrundande = brutto (alla pensionsgrundande rader)
        // Pension = pensionsgrundande * 6% (avrundat till ören)
        var expectedPension = Math.Round(result.Pensionsgrundande.Amount * 0.06m, 2, MidpointRounding.ToEven);
        Assert.Equal(expectedPension, result.Pensionsavgift.Amount);
    }

    [Fact]
    public async Task CalculateAsync_PensionOver7_5IBB_SplitRate()
    {
        // 80000 * 12 = 960 000 > 7.5 * 80 600 = 604 500
        SetupStandardEmployee(manadslon: 80000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Gräns per månad = 604 500 / 12 = 50 375
        var gransPerManad = 7.5m * 80600m / 12m;
        var underGrans = gransPerManad * 0.06m;
        var overGrans = (result.Pensionsgrundande.Amount - gransPerManad) * 0.315m;
        var expectedPension = Math.Round(underGrans + overGrans, 2, MidpointRounding.ToEven);

        Assert.Equal(expectedPension, result.Pensionsavgift.Amount);
        Assert.True(result.Pensionsavgift.Amount > result.Pensionsgrundande.Amount * 0.06m);
    }

    #endregion

    #region Jämkning

    [Fact]
    public async Task CalculateAsync_MedJamkning_ReduceradSkatt()
    {
        SetupStandardEmployee(harJamkning: true, jamkningBelopp: 2000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Skatt ska reduceras med jämkningsbelopp
        // Skatten från tabellen = 8000 (mock)
        // Jämkning = 2000
        // Netto skatt = 6000
        Assert.Equal(6000m, result.Skatt.Amount);
    }

    [Fact]
    public async Task CalculateAsync_JamkningStorreAnSkatt_SkattBlirNoll()
    {
        SetupStandardEmployee(harJamkning: true, jamkningBelopp: 999999m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Skatt ska inte vara negativ
        Assert.Equal(0m, result.Skatt.Amount);
    }

    #endregion

    #region Föräldralöneutfyllnad

    [Fact]
    public async Task CalculateAsync_MedForaldraledighet_CorrectUtfyllnad()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            ForaldraledigaDagar = 10
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Föräldralöneutfyllnad = dagslön * 10% * 10 dagar
        var daglon = 35000m * 100m / 100m / 21m;
        var expected = daglon * 0.10m * 10;
        Assert.Equal(expected, result.ForaldraloneUtfyllnad.Amount);

        var rad = result.Rader.First(r => r.LoneartKod == "3100");
        Assert.Equal("Föräldralöneutfyllnad", rad.Benamning);
        Assert.True(rad.ArSemestergrundande);
        Assert.True(rad.ArPensionsgrundande);
    }

    #endregion

    #region Kombinerat scenario

    [Fact]
    public async Task CalculateAsync_FulltScenario_CorrectBruttoOchNetto()
    {
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OBTimmar = [new OBInput { Kategori = OBCategory.VardagKvall, Timmar = 10 }],
            OvertidTimmar = 5,
            KvalificeradOvertid = false,
            SemesterdagarUttagna = 3,
            Kostnadsstalle = "1234"
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Grundlön = 35000
        // OB = 10 * 126.50 = 1265
        // Övertid = 5 * timlön * 0.8 (AB 25 supplement)
        var timlon = 35000m / (38.25m * 52m / 12m);
        var overtid = 5 * timlon * 0.8m;
        // Semestertillägg = 35000 * 100/100 * 0.0043 * 3 (AB 25)
        var semestertillagg = 35000m * 100m / 100m * 0.0043m * 3;

        var expectedBrutto = 35000m + 1265m + overtid + semestertillagg;
        Assert.Equal(expectedBrutto, result.Brutto.Amount);

        // Netto = Brutto - Skatt - avdrag
        Assert.True(result.Netto > Money.Zero);
        Assert.True(result.Netto < result.Brutto);
        Assert.Equal(0.3142m, result.ArbetsgivaravgiftSats);
    }

    #endregion

    #region Golden file test (öre-precision)

    [Fact]
    public async Task CalculateAsync_GoldenFileTest_KnownInputKnownOutput()
    {
        // Känd indata: 35 000 kr månadslön, heltid, 21 arbetsdagar,
        // 10h OB vardagkväll, 3 semesterdagar, skattetabell 33/1
        SetupStandardEmployee(manadslon: 35000m);
        var input = new PayrollInput
        {
            ArbetadeDagar = 21,
            ArbetsdagarIManadens = 21,
            OBTimmar = [new OBInput { Kategori = OBCategory.VardagKvall, Timmar = 10 }],
            SemesterdagarUttagna = 3,
            Kostnadsstalle = "1234"
        };

        var result = await _engine.CalculateAsync(
            _runId, _employeeId, _employmentId, 2025, 1, input);

        // Grundlön: exakt 35000.00
        Assert.Equal(35000.00m, result.Rader.First(r => r.LoneartKod == "1100").Belopp.Amount);

        // OB: 10 * 126.50 = 1265.00
        Assert.Equal(1265.00m, result.OBTillagg.Amount);

        // Semestertillägg: 35000 * 1.0 * 0.0043 * 3 = 451.50 (AB 25: 0.43%)
        Assert.Equal(451.50m, result.Semestertillagg.Amount);

        // Brutto: 35000 + 1265 + 451.50 = 36716.50
        Assert.Equal(36716.50m, result.Brutto.Amount);

        // Skatt: 8000 (mock tabell)
        Assert.Equal(8000m, result.Skatt.Amount);

        // Netto: 36716.50 - 8000 = 28716.50
        Assert.Equal(28716.50m, result.Netto.Amount);

        // Arbetsgivaravgifter: 36716.50 * 0.3142 = ~11536.24, avrundad till kronor
        var expectedAG = Math.Round(36716.50m * 0.3142m, 0, MidpointRounding.ToEven);
        Assert.Equal(expectedAG, result.Arbetsgivaravgifter.Amount);

        // Pension: alla pensionsgrundande rader * 6%
        Assert.True(result.Pensionsavgift.Amount > 0);
    }

    #endregion

    #region Mock implementations

    private sealed class MockTaxTableProvider : ITaxTableProvider
    {
        public Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default)
        {
            var table = new TaxTable
            {
                Id = 1,
                Ar = year,
                Tabellnummer = tableNumber,
                Kolumn = column
            };

            // Generera realistisk skattetabell
            table.LaggTillRad(new TaxTableRow { InkomstFran = 0m, InkomstTill = 19999m, Skattebelopp = 0m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 20000m, InkomstTill = 24999m, Skattebelopp = 3000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 25000m, InkomstTill = 29999m, Skattebelopp = 5000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 30000m, InkomstTill = 34999m, Skattebelopp = 7000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 35000m, InkomstTill = 39999m, Skattebelopp = 8000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 40000m, InkomstTill = 49999m, Skattebelopp = 10000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 50000m, InkomstTill = 59999m, Skattebelopp = 14000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 60000m, InkomstTill = 79999m, Skattebelopp = 18000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 80000m, InkomstTill = 99999m, Skattebelopp = 25000m });
            table.LaggTillRad(new TaxTableRow { InkomstFran = 100000m, InkomstTill = 999999m, Skattebelopp = 35000m });

            return Task.FromResult<TaxTable?>(table);
        }

        public Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<TaxTable>>(new List<TaxTable>());
        }
    }

    private sealed class MockCollectiveAgreementRulesEngine : ICollectiveAgreementRulesEngine
    {
        public Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, CancellationToken ct = default)
        {
            if (category == OBCategory.Ingen)
                return Task.FromResult(0m);

            var rate = (agreement, category) switch
            {
                (CollectiveAgreementType.AB, OBCategory.VardagKvall) => 126.50m,
                (CollectiveAgreementType.AB, OBCategory.VardagNatt) => 152.00m,
                (CollectiveAgreementType.AB, OBCategory.Helg) => 89.00m,
                (CollectiveAgreementType.AB, OBCategory.Storhelg) => 195.00m,
                _ => 0m
            };
            return Task.FromResult(rate);
        }

        public Task<OvertimeRules> GetOvertimeRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return Task.FromResult(new OvertimeRules
            {
                EnkelOvertidFaktor = 0.8m,
                KvalificeradOvertidFaktor = 1.4m,
                MaxOvertidPerVecka = 48m,
                MaxOvertidPerManad = 50m,
                MaxOvertidPerAr = 200m,
                KomptidFaktor = 1.5m
            });
        }

        public Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return GetVacationRulesAsync(agreement, date, fodelseAr: null, ct);
        }

        public Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, int? fodelseAr, CancellationToken ct = default)
        {
            var dagarPerAr = 25;
            if (fodelseAr.HasValue)
            {
                var alder = date.Year - fodelseAr.Value;
                dagarPerAr = alder switch
                {
                    >= 50 => 32,
                    >= 40 => 31,
                    _ => 25
                };
            }

            return Task.FromResult(new VacationRules
            {
                DagarPerAr = dagarPerAr,
                SammaloneregelProcent = 0.80m,
                SemestertillaggProcent = 0.43m,
                VariabelLonSemesterProcent = 12.0m,
                MaxSparadeDagar = 5,
                TotalMaxSparade = 40,
                IntjanandeArStart = new DateOnly(date.Year - 1, 4, 1),
                IntjanandeArSlut = new DateOnly(date.Year, 3, 31)
            });
        }

        public Task<SickPayRules> GetSickPayRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return Task.FromResult(new SickPayRules
            {
                KarensavdragProcent = 20m,
                SjuklonDag2Till14Procent = 80m,
                FKAnmalanEfterDag = 14,
                MaxSjuklonedagar = 14,
                LakarintygEfterDag = 7
            });
        }

        public Task<JourRegler> GetJourReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return Task.FromResult(new JourRegler
            {
                PassivTimlonFaktor = 0.40m,
                AktivTimlonFaktor = 1.5m
            });
        }

        public Task<BeredskapsRegler> GetBeredskapsReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return Task.FromResult(new BeredskapsRegler
            {
                PassivTimlonFaktor = 0.20m
            });
        }

        public Task<ForaldraloneRegler> GetForaldraloneReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
        {
            return Task.FromResult(new ForaldraloneRegler
            {
                DagarMedUtfyllnad = 180,
                UtfyllnadProcent = 0.10m
            });
        }
    }

    private sealed class MockCoreHRModule : ICoreHRModule
    {
        public EmployeeDto? Employee { get; set; }
        public EmploymentDto? Employment { get; set; }

        public Task<EmployeeDto?> GetEmployeeAsync(EmployeeId id, CancellationToken ct = default)
            => Task.FromResult(Employee);

        public Task<EmploymentDto?> GetActiveEmploymentAsync(EmployeeId id, DateOnly date, CancellationToken ct = default)
            => Task.FromResult(Employment);

        public Task<IReadOnlyList<EmploymentDto>> GetActiveEmploymentsAsync(EmployeeId id, DateOnly date, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<EmploymentDto>>(Employment is not null ? [Employment] : []);

        public Task<IReadOnlyList<EmployeeDto>> GetEmployeesByUnitAsync(OrganizationId unitId, DateOnly date, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<EmployeeDto>>(Employee is not null ? [Employee] : []);

        public Task<OrganizationUnitDto?> GetOrganizationUnitAsync(OrganizationId id, CancellationToken ct = default)
            => Task.FromResult<OrganizationUnitDto?>(null);
    }

    #endregion
}
