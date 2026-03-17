using RegionHR.Audit.Domain;

namespace RegionHR.Audit.Contracts;

public interface IAuditService
{
    Task LogAsync(
        string entityType,
        string entityId,
        AuditAction action,
        string? oldValues,
        string? newValues,
        string userId,
        string? userName = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<AuditEntry>> GetEntriesAsync(
        string? entityType = null,
        string? entityId = null,
        DateTime? from = null,
        DateTime? to = null,
        int take = 50,
        CancellationToken ct = default);
}
