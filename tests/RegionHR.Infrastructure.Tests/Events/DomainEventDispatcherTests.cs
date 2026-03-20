using Microsoft.Extensions.DependencyInjection;
using RegionHR.Infrastructure.Events;
using RegionHR.SharedKernel.Abstractions;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Events;

public class DomainEventDispatcherTests
{
    private record TestEvent(string Message) : DomainEvent;

    private class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> HandledEvents { get; } = [];

        public Task HandleAsync(TestEvent domainEvent, CancellationToken ct = default)
        {
            HandledEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_CallsMatchingHandler()
    {
        // Arrange
        var handler = new TestEventHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler);
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(provider);
        var testEvent = new TestEvent("hello");

        // Act
        await dispatcher.DispatchAsync([testEvent]);

        // Assert
        Assert.Single(handler.HandledEvents);
        Assert.Equal("hello", handler.HandledEvents[0].Message);
    }

    [Fact]
    public async Task DispatchAsync_WithNoHandler_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(provider);
        var testEvent = new TestEvent("orphan");

        // Act & Assert — should complete without throwing
        await dispatcher.DispatchAsync([testEvent]);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_DispatchesAll()
    {
        // Arrange
        var handler = new TestEventHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<TestEvent>>(handler);
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(provider);
        var events = new IDomainEvent[]
        {
            new TestEvent("first"),
            new TestEvent("second"),
            new TestEvent("third")
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        Assert.Equal(3, handler.HandledEvents.Count);
    }
}
