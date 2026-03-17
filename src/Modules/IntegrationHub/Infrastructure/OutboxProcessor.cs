using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.IntegrationHub.Infrastructure;

/// <summary>
/// Outbox Pattern implementation.
/// Säkerställer att integrationsmeddelanden levereras pålitligt.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Destination { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public DateTime? NextRetryAt { get; set; }
    public string? LastError { get; set; }
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
}

public enum OutboxStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    DeadLetter
}

/// <summary>
/// Bearbetar outbox-meddelanden med exponentiell backoff och dead-letter-kö.
/// </summary>
public sealed class OutboxProcessor
{
    private readonly Dictionary<string, IIntegrationAdapter> _adapters;
    // In production: IOutboxRepository
    private readonly List<OutboxMessage> _messages = [];

    public OutboxProcessor(IEnumerable<IIntegrationAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(a => a.SystemName);
    }

    public void Enqueue(string destination, string messageType, string payload)
    {
        _messages.Add(new OutboxMessage
        {
            Destination = destination,
            MessageType = messageType,
            Payload = payload
        });
    }

    /// <summary>Bearbeta alla väntande meddelanden.</summary>
    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var pending = _messages
            .Where(m => m.Status == OutboxStatus.Pending &&
                        (m.NextRetryAt == null || m.NextRetryAt <= DateTime.UtcNow))
            .OrderBy(m => m.CreatedAt)
            .ToList();

        foreach (var message in pending)
        {
            if (ct.IsCancellationRequested) break;

            if (!_adapters.TryGetValue(message.Destination, out var adapter))
            {
                message.Status = OutboxStatus.DeadLetter;
                message.LastError = $"Okänd destination: {message.Destination}";
                continue;
            }

            try
            {
                message.Status = OutboxStatus.Processing;
                var result = await adapter.ExecuteAsync(
                    new IntegrationRequest(message.MessageType, message.Payload), ct);

                if (result.Success)
                {
                    message.Status = OutboxStatus.Completed;
                    message.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    HandleFailure(message, result.Message ?? "Okänt fel");
                }
            }
            catch (Exception ex)
            {
                HandleFailure(message, ex.Message);
            }
        }
    }

    private static void HandleFailure(OutboxMessage message, string error)
    {
        message.RetryCount++;
        message.LastError = error;

        if (message.RetryCount >= message.MaxRetries)
        {
            message.Status = OutboxStatus.DeadLetter;
        }
        else
        {
            message.Status = OutboxStatus.Pending;
            // Exponentiell backoff: 1min, 4min, 16min, 64min, 256min
            var backoff = TimeSpan.FromMinutes(Math.Pow(4, message.RetryCount));
            message.NextRetryAt = DateTime.UtcNow + backoff;
        }
    }

    public IReadOnlyList<OutboxMessage> GetDeadLetterMessages() =>
        _messages.Where(m => m.Status == OutboxStatus.DeadLetter).ToList();
}
