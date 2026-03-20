using Microsoft.EntityFrameworkCore.Diagnostics;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Events;

/// <summary>
/// EF Core SaveChangesInterceptor that collects domain events from tracked entities
/// after SaveChanges and dispatches them via IDomainEventDispatcher.
/// </summary>
public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;

    public DomainEventInterceptor(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
        {
            DispatchDomainEventsAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavedChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(
        Microsoft.EntityFrameworkCore.DbContext context,
        CancellationToken ct)
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var entity = entry.Entity;
            var entityType = entity.GetType();

            var domainEventsProperty = entityType.GetProperty("DomainEvents");
            if (domainEventsProperty is null) continue;

            var events = domainEventsProperty.GetValue(entity) as IReadOnlyList<IDomainEvent>;
            if (events is null || events.Count == 0) continue;

            domainEvents.AddRange(events);

            var clearMethod = entityType.GetMethod("ClearDomainEvents");
            clearMethod?.Invoke(entity, null);
        }

        if (domainEvents.Count > 0)
        {
            await _dispatcher.DispatchAsync(domainEvents, ct);
        }
    }
}
