using RegionHR.Competence.Domain;

namespace RegionHR.Competence.Contracts;

public interface ICompetenceModule
{
    Task<IReadOnlyList<Certification>> GetCertificationsAsync(Guid anstallId, CancellationToken ct = default);
    Task<IReadOnlyList<Certification>> GetExpiringCertificationsAsync(int dagar = 90, CancellationToken ct = default);
}
