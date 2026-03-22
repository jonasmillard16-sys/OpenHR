using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Delivers domain events to matching webhook subscriptions via HTTP POST
/// with HMAC-SHA256 signing and exponential backoff retries.
/// </summary>
public class WebhookDeliveryService
{
    private readonly RegionHRDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDeliveryService> _logger;

    public WebhookDeliveryService(
        RegionHRDbContext db,
        HttpClient httpClient,
        ILogger<WebhookDeliveryService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _logger = logger;
    }

    /// <summary>
    /// Persist a domain event record and deliver to all matching subscriptions.
    /// </summary>
    public async Task DeliverAsync(string eventType, string aggregateType, Guid aggregateId, string data, CancellationToken ct = default)
    {
        // 1. Persist the domain event record
        var record = DomainEventRecord.Skapa(eventType, aggregateType, aggregateId, data);
        _db.DomainEventRecords.Add(record);
        await _db.SaveChangesAsync(ct);

        // 2. Find matching active subscriptions
        var subscriptions = await _db.EventSubscriptions
            .AsNoTracking()
            .Where(s => s.Status == EventSubscriptionStatus.Active)
            .ToListAsync(ct);

        var matching = subscriptions.Where(s => s.MatcharEventTyp(eventType)).ToList();

        // 3. Create delivery records and attempt delivery
        foreach (var subscription in matching)
        {
            var delivery = EventDelivery.Skapa(subscription.Id, record.Id);
            _db.EventDeliveries.Add(delivery);
            await _db.SaveChangesAsync(ct);

            await AttemptDeliveryAsync(subscription, delivery, record, ct);
        }
    }

    /// <summary>
    /// Attempt HTTP delivery to a webhook endpoint.
    /// </summary>
    private async Task AttemptDeliveryAsync(
        EventSubscription subscription,
        EventDelivery delivery,
        DomainEventRecord record,
        CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                eventId = record.Id,
                type = record.Typ,
                aggregateType = record.AggregatTyp,
                aggregateId = record.AggregatId,
                data = record.Data,
                timestamp = record.SkapadVid
            });

            var signature = ComputeHmacSignature(payload, subscription.HemligNyckel);

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            request.Headers.Add("X-OpenHR-Signature", $"sha256={signature}");
            request.Headers.Add("X-OpenHR-Event-Type", record.Typ);

            var response = await _httpClient.SendAsync(request, ct);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                delivery.MarkeraLeverad(statusCode);
                subscription.AterstallMisslyckanden();
                _logger.LogInformation("Webhook delivered to {Url} for event {EventType} (HTTP {StatusCode})",
                    subscription.Url, record.Typ, statusCode);
            }
            else
            {
                delivery.MarkeraMisslyckad(statusCode);
                subscription.OkaMisslyckanden();
                _logger.LogWarning("Webhook delivery failed to {Url} for event {EventType} (HTTP {StatusCode})",
                    subscription.Url, record.Typ, statusCode);
            }
        }
        catch (Exception ex)
        {
            delivery.MarkeraMisslyckad(0);
            subscription.OkaMisslyckanden();
            _logger.LogError(ex, "Webhook delivery error to {Url} for event {EventType}",
                subscription.Url, record.Typ);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Compute HMAC-SHA256 signature for webhook payload verification.
    /// </summary>
    public static string ComputeHmacSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Retry a failed delivery.  Loads the related subscription and event record,
    /// then re-attempts HTTP delivery and updates the delivery status.
    /// </summary>
    public async Task<bool> RedeliverAsync(EventDelivery delivery, CancellationToken ct = default)
    {
        var subscription = await _db.EventSubscriptions.FindAsync(
            new object[] { delivery.EventSubscriptionId }, ct);
        if (subscription is null)
        {
            _logger.LogWarning("Webhook retry: prenumeration {Id} hittades inte", delivery.EventSubscriptionId);
            return false;
        }

        var record = await _db.DomainEventRecords.FindAsync(
            new object[] { delivery.DomainEventRecordId }, ct);
        if (record is null)
        {
            _logger.LogWarning("Webhook retry: händelsepost {Id} hittades inte", delivery.DomainEventRecordId);
            return false;
        }

        await AttemptDeliveryAsync(subscription, delivery, record, ct);
        return delivery.Status == EventDeliveryStatus.Delivered;
    }

    /// <summary>
    /// Test delivery to a subscription with a sample event.
    /// </summary>
    public async Task<(bool success, int statusCode)> TestDeliveryAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _db.EventSubscriptions.FindAsync(new object[] { subscriptionId }, ct);
        if (subscription is null)
            return (false, 0);

        var testPayload = JsonSerializer.Serialize(new
        {
            eventId = Guid.NewGuid(),
            type = "test.ping",
            aggregateType = "System",
            aggregateId = Guid.Empty,
            data = "{}",
            timestamp = DateTime.UtcNow
        });

        var signature = ComputeHmacSignature(testPayload, subscription.HemligNyckel);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url);
            request.Content = new StringContent(testPayload, Encoding.UTF8, "application/json");
            request.Headers.Add("X-OpenHR-Signature", $"sha256={signature}");
            request.Headers.Add("X-OpenHR-Event-Type", "test.ping");

            var response = await _httpClient.SendAsync(request, ct);
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch
        {
            return (false, 0);
        }
    }
}
