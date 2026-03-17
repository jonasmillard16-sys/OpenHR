using RegionHR.SharedKernel.Domain;

namespace RegionHR.Core.Contracts;

/// <summary>
/// Publikt kontrakt för Core HR-modulen.
/// Andra moduler anropar detta interface -- aldrig direkt databasåtkomst.
/// </summary>
public interface ICoreHRModule
{
    Task<EmployeeDto?> GetEmployeeAsync(EmployeeId id, CancellationToken ct = default);
    Task<EmploymentDto?> GetActiveEmploymentAsync(EmployeeId id, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<EmploymentDto>> GetActiveEmploymentsAsync(EmployeeId id, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDto>> GetEmployeesByUnitAsync(OrganizationId unitId, DateOnly date, CancellationToken ct = default);
    Task<OrganizationUnitDto?> GetOrganizationUnitAsync(OrganizationId id, CancellationToken ct = default);
}

public record EmployeeDto(
    EmployeeId Id,
    string Fornamn,
    string Efternamn,
    string PersonnummerMaskerat,
    string? Epost,
    int? Skattetabell,
    int? Skattekolumn,
    string? Kommun,
    decimal? KommunalSkattesats,
    bool HarKyrkoavgift,
    decimal? Kyrkoavgiftssats,
    bool HarJamkning,
    decimal? JamkningBelopp);

public record EmploymentDto(
    EmploymentId Id,
    EmployeeId AnstallId,
    OrganizationId EnhetId,
    EmploymentType Anstallningsform,
    CollectiveAgreementType Kollektivavtal,
    decimal Manadslon,
    decimal Sysselsattningsgrad,
    DateOnly StartDatum,
    DateOnly? SlutDatum,
    string? BESTAKod);

public record OrganizationUnitDto(
    OrganizationId Id,
    string Namn,
    OrganizationUnitType Typ,
    string Kostnadsstalle,
    OrganizationId? OverordnadEnhetId);
