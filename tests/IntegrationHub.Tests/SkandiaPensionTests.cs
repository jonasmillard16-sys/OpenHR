using Xunit;
using RegionHR.IntegrationHub.Adapters.Skandia;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Tests;

public class SkandiaPensionTests
{
    [Fact]
    public void BeraknaAKAPKRPremier_UnderGrans_Returns6Percent()
    {
        // Arrange - lön under 7.5 IBB / 12 (80600 * 7.5 / 12 = 50375)
        var lon = 30000m;

        // Act
        var (underGrans, overGrans) = SkandiaPensionAdapter.BeraknaAKAPKRPremier(lon);

        // Assert
        Assert.Equal(lon * 0.06m, underGrans);  // 6% av hela lönen
        Assert.Equal(0m, overGrans);              // Inget över gränsen
    }

    [Fact]
    public void BeraknaAKAPKRPremier_OverGrans_Splits6And31Point5Percent()
    {
        // Arrange - lön över 7.5 IBB / 12
        // 7.5 IBB = 80600 * 7.5 = 604500, per månad = 50375
        var manadsGrans = 80600m * 7.5m / 12m;
        var lon = 70000m;

        // Act
        var (underGrans, overGrans) = SkandiaPensionAdapter.BeraknaAKAPKRPremier(lon);

        // Assert
        var forvantatUnder = manadsGrans * 0.06m;
        var forvantatOver = (lon - manadsGrans) * 0.315m;

        Assert.Equal(forvantatUnder, underGrans);
        Assert.Equal(forvantatOver, overGrans);
        Assert.True(overGrans > 0, "Avgift över gränsen bör vara > 0");
    }

    [Fact]
    public void GenereraPensionsrapport_ValidInput_ReturnsPipeSeparatedFormat()
    {
        // Arrange
        var adapter = new SkandiaPensionAdapter();
        var input = new PensionsrapportInput
        {
            Period = "202603",
            Individer =
            [
                new PensionsIndivid
                {
                    Personnummer = "198501011234",
                    PensionsgrundandeLon = 35000m
                },
                new PensionsIndivid
                {
                    Personnummer = "199001025678",
                    PensionsgrundandeLon = 65000m
                }
            ]
        };

        // Act
        var rapport = adapter.GenereraPipeSeparatedRapport(input);

        // Assert
        var lines = rapport.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Header
        Assert.StartsWith("H|AKAP-KR|202603|2|RegionHR", lines[0]);

        // Detail lines start with D
        Assert.StartsWith("D|198501011234|", lines[1]);
        Assert.StartsWith("D|199001025678|", lines[2]);

        // Footer starts with T
        Assert.StartsWith("T|2|", lines[3]);

        // Verify pipe separation
        Assert.Equal(6, lines[1].Split('|').Length); // D|Pnr|Lon|Under|Over|Total
    }

    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsError()
    {
        // Arrange
        var adapter = new SkandiaPensionAdapter();
        var input = new PensionsrapportInput
        {
            Period = "202603",
            Individer = []
        };
        var request = new IntegrationRequest("GenereraPensionsrapport", input);

        // Act
        var result = await adapter.ExecuteAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Inga individer", result.Message);
    }
}
