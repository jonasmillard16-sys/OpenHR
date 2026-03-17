using RegionHR.Leave.Domain;

namespace RegionHR.Leave.Contracts;

/// <summary>
/// Publikt kontrakt fr Leave-modulen (Semester/Frnvaro).
/// Andra moduler anropar detta interface -- aldrig direkt databastkomst.
/// </summary>
public interface ILeaveModule
{
    Task<VacationBalance?> GetBalanceAsync(Guid anstallId, int ar, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetRequestsAsync(Guid anstallId, CancellationToken ct = default);
}
