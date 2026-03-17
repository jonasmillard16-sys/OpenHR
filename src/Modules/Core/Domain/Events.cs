using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Core.Domain;

public sealed record EmployeeCreatedEvent(EmployeeId EmployeeId, string PersonnummerMasked) : DomainEvent;
public sealed record EmploymentCreatedEvent(EmploymentId EmploymentId, EmployeeId EmployeeId, EmploymentType Type) : DomainEvent;
public sealed record EmploymentEndedEvent(EmploymentId EmploymentId, EmployeeId EmployeeId, DateOnly SlutDatum) : DomainEvent;
public sealed record SalaryChangedEvent(EmploymentId EmploymentId, Money OldSalary, Money NewSalary) : DomainEvent;
