namespace RegionHR.GDPR.Domain;

public class RetentionRecord
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public DateTime RetentionExpires { get; private set; }
    public string RetentionReason { get; private set; } = default!;
    public bool IsAnonymized { get; private set; }
    public DateTime? AnonymizedAt { get; private set; }

    private RetentionRecord() { }

    public static RetentionRecord Skapa(string entityType, string entityId, DateTime expires, string reason)
    {
        return new RetentionRecord
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            RetentionExpires = expires,
            RetentionReason = reason,
            IsAnonymized = false
        };
    }

    public void Anonymize()
    {
        IsAnonymized = true;
        AnonymizedAt = DateTime.UtcNow;
    }
}
