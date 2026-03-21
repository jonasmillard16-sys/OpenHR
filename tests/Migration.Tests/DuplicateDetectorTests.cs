using RegionHR.Migration.Adapters;
using RegionHR.Migration.Services;
using Xunit;

namespace RegionHR.Migration.Tests;

public class DuplicateDetectorTests
{
    private readonly DuplicateDetector _detector = new();

    [Fact]
    public void FindDuplicates_MatchandePnr_ReturnDuplicate()
    {
        var existingId = Guid.NewGuid();
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = existingId
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "198501151234",
                    ["Fornamn"] = "Anna",
                    ["Efternamn"] = "Svensson"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Single(duplicates);
        Assert.Equal(existingId, duplicates[0].ExistingEmployeeId);
        Assert.Equal("198501151234", duplicates[0].Personnummer);
        Assert.Equal(0, duplicates[0].RecordIndex);
    }

    [Fact]
    public void FindDuplicates_IngenMatch_TomLista()
    {
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = Guid.NewGuid()
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "199003221567",
                    ["Fornamn"] = "Erik",
                    ["Efternamn"] = "Johansson"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void FindDuplicates_MedBindestreck_NormaliserarOchMatchar()
    {
        var existingId = Guid.NewGuid();
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = existingId
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "19850115-1234",
                    ["Fornamn"] = "Anna",
                    ["Efternamn"] = "Svensson"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Single(duplicates);
    }

    [Fact]
    public void FindDuplicates_TioSiffrigt_NormaliserarMedArhundrade()
    {
        var existingId = Guid.NewGuid();
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = existingId
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "8501151234",
                    ["Fornamn"] = "Anna",
                    ["Efternamn"] = "Svensson"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Single(duplicates);
    }

    [Fact]
    public void FindDuplicates_PersnrFaltnamn_Matchar()
    {
        var existingId = Guid.NewGuid();
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = existingId
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["PERSNR"] = "198501151234",
                    ["Fornamn"] = "Anna",
                    ["Efternamn"] = "Svensson"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Single(duplicates);
    }

    [Fact]
    public void FindDuplicates_HopparOverNonEmployeeRecords()
    {
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = Guid.NewGuid()
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "PayrollRecord",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "198501151234",
                    ["Loneart"] = "1000"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void FindDuplicates_FleraMatchningar_AllaReturneras()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var lookup = new Dictionary<string, Guid>
        {
            ["198501151234"] = id1,
            ["199003221567"] = id2
        };

        var records = new List<ParsedRecord>
        {
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "198501151234",
                    ["Fornamn"] = "Anna",
                    ["Efternamn"] = "Svensson"
                }
            },
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "199003221567",
                    ["Fornamn"] = "Erik",
                    ["Efternamn"] = "Johansson"
                }
            },
            new()
            {
                EntityType = "Employee",
                Fields = new Dictionary<string, string>
                {
                    ["Personnummer"] = "200105150987",
                    ["Fornamn"] = "Sara",
                    ["Efternamn"] = "Lindberg"
                }
            }
        };

        var duplicates = _detector.FindDuplicates(lookup, records);

        Assert.Equal(2, duplicates.Count);
        Assert.Contains(duplicates, d => d.ExistingEmployeeId == id1 && d.RecordIndex == 0);
        Assert.Contains(duplicates, d => d.ExistingEmployeeId == id2 && d.RecordIndex == 1);
    }

    [Fact]
    public void NormalizePnr_12Siffror_ReturnerarDirekt()
    {
        var result = DuplicateDetector.NormalizePnr("198501151234");

        Assert.Equal("198501151234", result);
    }

    [Fact]
    public void NormalizePnr_10Siffror_LaggerTillArhundrade()
    {
        var result = DuplicateDetector.NormalizePnr("8501151234");

        Assert.Equal("198501151234", result);
    }

    [Fact]
    public void NormalizePnr_MedBindestreck_Rensas()
    {
        var result = DuplicateDetector.NormalizePnr("19850115-1234");

        Assert.Equal("198501151234", result);
    }

    [Fact]
    public void NormalizePnr_OgiltigLangd_ReturnerarTom()
    {
        var result = DuplicateDetector.NormalizePnr("12345");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizePnr_TomStrang_ReturnerarTom()
    {
        var result = DuplicateDetector.NormalizePnr("");

        Assert.Equal(string.Empty, result);
    }
}
