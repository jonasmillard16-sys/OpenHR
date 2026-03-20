using RegionHR.Infrastructure.Services;
using Xunit;

namespace RegionHR.Platform.Tests;

public class WebhookDeliveryServiceTests
{
    [Fact]
    public void ComputeHmacSignature_ProducesDeterministicSignature()
    {
        var sig1 = WebhookDeliveryService.ComputeHmacSignature("payload", "secret");
        var sig2 = WebhookDeliveryService.ComputeHmacSignature("payload", "secret");

        Assert.Equal(sig1, sig2);
        Assert.Equal(64, sig1.Length); // HMAC-SHA256 hex = 64 chars
    }

    [Fact]
    public void ComputeHmacSignature_DifferentPayloadsProduceDifferentSignatures()
    {
        var sig1 = WebhookDeliveryService.ComputeHmacSignature("payload1", "secret");
        var sig2 = WebhookDeliveryService.ComputeHmacSignature("payload2", "secret");

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void ComputeHmacSignature_DifferentSecretsProduceDifferentSignatures()
    {
        var sig1 = WebhookDeliveryService.ComputeHmacSignature("payload", "secret1");
        var sig2 = WebhookDeliveryService.ComputeHmacSignature("payload", "secret2");

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void ComputeHmacSignature_IsLowerHex()
    {
        var sig = WebhookDeliveryService.ComputeHmacSignature("test", "key");

        Assert.Matches("^[0-9a-f]{64}$", sig);
    }
}
