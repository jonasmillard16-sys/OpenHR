namespace RegionHR.Audit.Domain;

public enum AuditAction
{
    Create,
    Update,
    Delete
}

public sealed class AuditEntry
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public AuditAction Action { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string UserId { get; private set; } = default!;
    public string? UserName { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditEntry() { }

    public static AuditEntry Create(
        string entityType,
        string entityId,
        AuditAction action,
        string? oldValues,
        string? newValues,
        string userId,
        string? userName,
        string? ipAddress)
    {
        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };
    }
}
