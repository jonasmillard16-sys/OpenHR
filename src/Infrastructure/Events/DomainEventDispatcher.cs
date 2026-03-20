using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Services;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod("HandleAsync");
                if (method is null) continue;
                await (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
            }

            // After in-process handlers, deliver to external webhooks
            try
            {
                var webhookService = _serviceProvider.GetService<WebhookDeliveryService>();
                if (webhookService is not null)
                {
                    var typeName = eventType.Name;
                    // Convert PascalCase to dot.notation, e.g. EmployeeCreated -> employee.created
                    var eventTypeName = ToDotNotation(typeName);
                    var aggregateType = eventType.Namespace?.Split('.').LastOrDefault() ?? "Unknown";
                    var data = JsonSerializer.Serialize(domainEvent, eventType);

                    await webhookService.DeliverAsync(
                        eventTypeName,
                        aggregateType,
                        domainEvent.EventId,
                        data,
                        ct);
                }
            }
            catch (Exception ex)
            {
                // Webhook delivery failures should not break the main flow
                var logger = _serviceProvider.GetService<ILogger<DomainEventDispatcher>>();
                logger?.LogWarning(ex, "Webhook delivery failed for event {EventType}", eventType.Name);
            }
        }
    }

    /// <summary>
    /// Convert PascalCase to dot.notation: EmployeeCreated -> employee.created
    /// </summary>
    internal static string ToDotNotation(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase)) return pascalCase;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < pascalCase.Length; i++)
        {
            var c = pascalCase[i];
            if (char.IsUpper(c) && i > 0)
            {
                result.Append('.');
            }
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }
}
