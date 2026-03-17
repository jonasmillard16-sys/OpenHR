using Xunit;
using RegionHR.IntegrationHub.Adapters.Forsakringskassan;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Tests;

public class ForsakringskassanTests
{
    [Fact]
    public async Task SkickaFKSjukanmalan_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var adapter = new ForsakringskassanAdapter();
        var sjukanmalan = new FKSjukanmalan
        {
            Personnummer = "198501011234",
            SjukfranvaroStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
            SjukfranvaroSlut = null,
            Arbetsgivare = "Region Test"
        };
        var request = new IntegrationRequest("SkickaFKSjukanmalan", sjukanmalan);

        // Act
        var result = await adapter.ExecuteAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("FK", result.Message);
        Assert.NotNull(result.ResponseData);
    }

    [Fact]
    public async Task SkickaFKSjukanmalan_Under14Dagar_ReturnsError()
    {
        // Arrange
        var adapter = new ForsakringskassanAdapter();
        var sjukanmalan = new FKSjukanmalan
        {
            Personnummer = "198501011234",
            SjukfranvaroStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            SjukfranvaroSlut = DateOnly.FromDateTime(DateTime.Today),
            Arbetsgivare = "Region Test"
        };
        var request = new IntegrationRequest("SkickaFKSjukanmalan", sjukanmalan);

        // Act
        var result = await adapter.ExecuteAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("dag 14", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownOperation_ReturnsError()
    {
        // Arrange
        var adapter = new ForsakringskassanAdapter();
        var request = new IntegrationRequest("OkandOperation", "test");

        // Act
        var result = await adapter.ExecuteAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Okänd operation", result.Message);
    }

    [Fact]
    public async Task HealthCheck_ReturnsTrue()
    {
        // Arrange
        var adapter = new ForsakringskassanAdapter();

        // Act
        var result = await adapter.HealthCheckAsync();

        // Assert
        Assert.True(result);
    }
}
