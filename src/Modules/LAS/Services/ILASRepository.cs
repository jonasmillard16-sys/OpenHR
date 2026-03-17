using RegionHR.LAS.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.LAS.Services;

/// <summary>
/// Repository-kontrakt för LAS-ackumuleringar.
/// </summary>
public interface ILASRepository
{
    Task<LASAccumulation?> GetByEmployeeAsync(EmployeeId id, CancellationToken ct);
    Task<IReadOnlyList<LASAccumulation>> GetAllaAktiva(CancellationToken ct);
    Task<IReadOnlyList<LASAccumulation>> GetByStatus(LASStatus status, CancellationToken ct);
    Task AddAsync(LASAccumulation acc, CancellationToken ct);
    Task UpdateAsync(LASAccumulation acc, CancellationToken ct);
}
