using RegionHR.HalsoSAM.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.HalsoSAM.Services;

/// <summary>
/// Repository-kontrakt för rehabiliteringsärenden.
/// </summary>
public interface IRehabRepository
{
    Task<RehabCase?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<RehabCase>> GetByEmployeeAsync(EmployeeId anstallId, CancellationToken ct);
    Task<IReadOnlyList<RehabCase>> GetByStatusAsync(RehabStatus status, CancellationToken ct);
    Task<IReadOnlyList<RehabCase>> GetAktivaAsync(CancellationToken ct);
    Task<IReadOnlyList<RehabCase>> GetAllAsync(CancellationToken ct);
    Task AddAsync(RehabCase rehabCase, CancellationToken ct);
    Task UpdateAsync(RehabCase rehabCase, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
