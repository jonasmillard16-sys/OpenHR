using RegionHR.Migration.Adapters;
using RegionHR.Migration.Services;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Migration.Tests;

public class MigrationValidatorTests
{
    private readonly MigrationValidator _validator = new();
    private readonly MigrationJobId _jobId = MigrationJobId.New();

    // ====================================
    // Personnummer validation
    // ====================================

    [Fact]
    public void ValidateRecords_GiltigtPersonnummer12Siffror_IngetFel()
    {
        // 198501151234 — must be valid Luhn. Let's compute one:
        // Use Personnummer.CreateValidated to get a known-good one
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord(pnr.ToString().Replace("-", ""))
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        Assert.DoesNotContain(errors, e => e.Falt == "Personnummer");
    }

    [Fact]
    public void ValidateRecords_ForKortPersonnummer_GerFel()
    {
        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord("12345")
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        Assert.Contains(errors, e =>
            e.Falt == "Personnummer" && e.FelTyp == "OgiltigLängd");
    }

    [Fact]
    public void ValidateRecords_ForLangtPersonnummer_GerFel()
    {
        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord("1234567890123")
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        Assert.Contains(errors, e =>
            e.Falt == "Personnummer" && e.FelTyp == "OgiltigLängd");
    }

    [Fact]
    public void ValidateRecords_OgiltigKontrollsiffra_GerFelMedForslag()
    {
        // Construct a personnummer with wrong check digit
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var valid = pnr.ToString().Replace("-", ""); // 12 digits, valid
        // Change last digit to make it invalid
        var lastDigit = valid[^1] - '0';
        var wrongDigit = (lastDigit + 1) % 10;
        var invalid = valid[..^1] + wrongDigit;

        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord(invalid)
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        var pnrError = Assert.Single(errors, e =>
            e.Falt == "Personnummer" && e.FelTyp == "OgiltigKontrollsiffra");
        Assert.NotNull(pnrError.ForeslagnKorrektion);
        Assert.Contains("menade du", pnrError.ForeslagnKorrektion);
    }

    [Fact]
    public void ValidateRecords_DubblaPersonnummer_GerFel()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var pnrStr = pnr.ToString().Replace("-", "");
        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord(pnrStr),
            CreateEmployeeRecord(pnrStr)
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        Assert.Contains(errors, e =>
            e.Falt == "Personnummer" && e.FelTyp == "Dubblett");
    }

    [Fact]
    public void ValidateRecords_PersnrFalt_ValidatesCorrectly()
    {
        // Test that PERSNR field name is also detected
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19900322156");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["PERSNR"] = "12345",
                ["Fornamn"] = "Test",
                ["Efternamn"] = "Testsson"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.Contains(errors, e => e.Falt == "Personnummer" && e.FelTyp == "OgiltigLängd");
    }

    [Fact]
    public void ValidateRecords_PersonnummerMedBindestreck_Accepteras()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        // Insert a dash: YYYYMMDD-NNNN
        var withDash = pnr.ToString(); // Already has dash from Personnummer.ToString()
        var records = new List<ParsedRecord>
        {
            CreateEmployeeRecord(withDash)
        };

        var errors = _validator.ValidateRecords(_jobId, records);

        Assert.DoesNotContain(errors, e =>
            e.Falt == "Personnummer" && e.FelTyp == "OgiltigLängd");
    }

    // ====================================
    // Required fields
    // ====================================

    [Fact]
    public void ValidateRecords_SaknarFornamn_GerFel()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = pnr.ToString().Replace("-", ""),
                ["Efternamn"] = "Svensson"
                // Fornamn is missing
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.Contains(errors, e =>
            e.Falt == "Fornamn" && e.FelTyp == "ObligatorisktFältSaknas");
    }

    [Fact]
    public void ValidateRecords_SaknarEfternamn_GerFel()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = pnr.ToString().Replace("-", ""),
                ["Fornamn"] = "Anna"
                // Efternamn is missing
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.Contains(errors, e =>
            e.Falt == "Efternamn" && e.FelTyp == "ObligatorisktFältSaknas");
    }

    // ====================================
    // Date validation
    // ====================================

    [Theory]
    [InlineData("2026-03-21")]
    [InlineData("20260321")]
    [InlineData("26-03-21")]
    [InlineData("260321")]
    public void TryParseDate_GiltigaDatum_Accepteras(string date)
    {
        Assert.True(MigrationValidator.TryParseDate(date));
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("2026-13-01")]
    [InlineData("abc")]
    public void TryParseDate_OgiltigaDatum_Avvisas(string date)
    {
        Assert.False(MigrationValidator.TryParseDate(date));
    }

    [Fact]
    public void ValidateRecords_OgiltigtDatum_GerFel()
    {
        var record = new ParsedRecord
        {
            EntityType = "TimeRecord",
            Fields = new Dictionary<string, string>
            {
                ["Datum"] = "not-a-date",
                ["Tidkod"] = "NARV"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.Contains(errors, e =>
            e.Falt == "Datum" && e.FelTyp == "OgiltigtDatumformat");
    }

    // ====================================
    // Amount validation
    // ====================================

    [Fact]
    public void ValidateRecords_OgiltigtBelopp_GerFel()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = pnr.ToString().Replace("-", ""),
                ["Fornamn"] = "Anna",
                ["Efternamn"] = "Svensson",
                ["Manadslon"] = "abc-not-a-number"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.Contains(errors, e =>
            e.Falt == "Manadslon" && e.FelTyp == "OgiltigtBelopp");
    }

    [Fact]
    public void ValidateRecords_GiltigtBelopp_IngetFel()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = pnr.ToString().Replace("-", ""),
                ["Fornamn"] = "Anna",
                ["Efternamn"] = "Svensson",
                ["Manadslon"] = "35000"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.DoesNotContain(errors, e => e.Falt == "Manadslon");
    }

    [Fact]
    public void ValidateRecords_BeloppMedKomma_Accepteras()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var record = new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = pnr.ToString().Replace("-", ""),
                ["Fornamn"] = "Anna",
                ["Efternamn"] = "Svensson",
                ["Manadslon"] = "35 000,50"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.DoesNotContain(errors, e => e.Falt == "Manadslon");
    }

    // ====================================
    // Luhn algorithm
    // ====================================

    [Fact]
    public void LuhnCheck_GiltigPersonnummer_ReturnerarTrue()
    {
        // 8507301612 is a commonly used test personnummer (YYMMDDNNNC)
        // Let's use our CreateValidated to get one
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var tenDigit = pnr.ToString().Replace("-", "")[2..]; // Remove century

        Assert.True(MigrationValidator.LuhnCheck(tenDigit));
    }

    [Fact]
    public void LuhnCheck_OgiltigSiffra_ReturnerarFalse()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var tenDigit = pnr.ToString().Replace("-", "")[2..];
        // Flip last digit
        var lastDigit = tenDigit[^1] - '0';
        var wrong = tenDigit[..^1] + ((lastDigit + 1) % 10);

        Assert.False(MigrationValidator.LuhnCheck(wrong));
    }

    [Fact]
    public void TryCorrectCheckDigit_ReturnsCorrected()
    {
        var pnr = RegionHR.SharedKernel.Domain.Personnummer.CreateValidated("19850115123");
        var tenDigit = pnr.ToString().Replace("-", "")[2..];
        // Corrupt last digit
        var lastDigit = tenDigit[^1] - '0';
        var wrong = tenDigit[..^1] + ((lastDigit + 1) % 10);

        var corrected = MigrationValidator.TryCorrectCheckDigit(wrong);

        Assert.NotNull(corrected);
        Assert.Equal(tenDigit, corrected);
    }

    // ====================================
    // Non-Employee records
    // ====================================

    [Fact]
    public void ValidateRecords_IckeEmployeeRecord_ValiderarInteFornamnEfternamn()
    {
        var record = new ParsedRecord
        {
            EntityType = "PayrollRecord",
            Fields = new Dictionary<string, string>
            {
                ["Loneart"] = "1000",
                ["Belopp"] = "35000"
            }
        };

        var errors = _validator.ValidateRecords(_jobId, new List<ParsedRecord> { record });

        Assert.DoesNotContain(errors, e => e.FelTyp == "ObligatorisktFältSaknas");
    }

    // ====================================
    // Helper
    // ====================================

    private static ParsedRecord CreateEmployeeRecord(string personnummer)
    {
        return new ParsedRecord
        {
            EntityType = "Employee",
            Fields = new Dictionary<string, string>
            {
                ["Personnummer"] = personnummer,
                ["Fornamn"] = "Test",
                ["Efternamn"] = "Testsson"
            }
        };
    }
}
