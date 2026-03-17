using RegionHR.GDPR.Domain;

namespace RegionHR.GDPR.Services;

public interface IGDPRService
{
    Task<DataSubjectRequest> CreateRequestAsync(Guid anstallId, RequestType typ, CancellationToken ct = default);
    Task<IReadOnlyList<DataSubjectRequest>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RetentionRecord>> GetExpiredRetentionAsync(CancellationToken ct = default);
}
