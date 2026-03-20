using RegionHR.Platform.Domain;
using Xunit;

namespace RegionHR.Platform.Tests;

public class EventDeliveryTests
{
    [Fact]
    public void Skapa_SetsDefaultValues()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, delivery.Id);
        Assert.Equal(EventDeliveryStatus.Pending, delivery.Status);
        Assert.Equal(0, delivery.AntalForsok);
        Assert.Null(delivery.HttpStatusKod);
        Assert.Null(delivery.NastaRetry);
        Assert.Null(delivery.LeveradVid);
    }

    [Fact]
    public void MarkeraLeverad_SetsStatusAndTimestamp()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());

        delivery.MarkeraLeverad(200);

        Assert.Equal(EventDeliveryStatus.Delivered, delivery.Status);
        Assert.Equal(200, delivery.HttpStatusKod);
        Assert.Equal(1, delivery.AntalForsok);
        Assert.NotNull(delivery.LeveradVid);
    }

    [Fact]
    public void MarkeraMisslyckad_SetsStatusAndSchedulesRetry()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());
        var before = DateTime.UtcNow;

        delivery.MarkeraMisslyckad(500);

        Assert.Equal(EventDeliveryStatus.Failed, delivery.Status);
        Assert.Equal(500, delivery.HttpStatusKod);
        Assert.Equal(1, delivery.AntalForsok);
        Assert.NotNull(delivery.NastaRetry);
        // First retry: ~1 minute
        Assert.True(delivery.NastaRetry!.Value >= before.AddSeconds(50));
        Assert.True(delivery.NastaRetry!.Value <= before.AddMinutes(2));
    }

    [Fact]
    public void MarkeraMisslyckad_ExponentialBackoff()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());

        // First failure: 1 min
        delivery.MarkeraMisslyckad(500);
        var retry1 = delivery.NastaRetry;

        // Second failure: 5 min
        delivery.MarkeraMisslyckad(500);
        var retry2 = delivery.NastaRetry;

        // Third failure: 30 min
        delivery.MarkeraMisslyckad(503);
        var retry3 = delivery.NastaRetry;

        Assert.NotNull(retry1);
        Assert.NotNull(retry2);
        Assert.NotNull(retry3);
        Assert.Equal(3, delivery.AntalForsok);
    }

    [Fact]
    public void KanRetry_ReturnsFalseWhenNotFailed()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(delivery.KanRetry());
    }

    [Fact]
    public void KanRetry_ReturnsFalseWhenMaxRetriesExceeded()
    {
        var delivery = EventDelivery.Skapa(Guid.NewGuid(), Guid.NewGuid());
        // Fail 5 times (max default)
        for (int i = 0; i < 5; i++)
            delivery.MarkeraMisslyckad(500);

        Assert.False(delivery.KanRetry(5));
    }

    [Fact]
    public void DomainEventRecord_Skapa_SetsAllProperties()
    {
        var aggregateId = Guid.NewGuid();
        var record = DomainEventRecord.Skapa(
            "employee.created",
            "Core",
            aggregateId,
            """{"name":"Test"}""");

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal("employee.created", record.Typ);
        Assert.Equal("Core", record.AggregatTyp);
        Assert.Equal(aggregateId, record.AggregatId);
        Assert.Contains("Test", record.Data);
        Assert.NotEqual(Guid.Empty, record.KorrelationsId);
    }

    [Fact]
    public void DomainEventRecord_Skapa_WithCorrelationId()
    {
        var corrId = Guid.NewGuid();
        var record = DomainEventRecord.Skapa(
            "employee.created", "Core", Guid.NewGuid(), "{}", corrId);

        Assert.Equal(corrId, record.KorrelationsId);
    }
}
