using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Budget per organisationsenhet inom en kompensationsplan.
/// </summary>
public sealed class CompensationBudget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public CompensationPlanId CompensationPlanId { get; set; }
    public OrganizationId OrganizationUnitId { get; set; }
    public decimal TotalUtrymme { get; set; }
    public decimal Fordelat { get; set; }
    public decimal Kvar => TotalUtrymme - Fordelat;
}
