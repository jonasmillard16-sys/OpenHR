using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Riktlinje for lonejustering baserat pa prestationsniva.
/// </summary>
public sealed class CompensationGuideline
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public CompensationPlanId CompensationPlanId { get; set; }
    public string PrestationsNiva { get; set; } = string.Empty;
    public decimal RekommenderadHojningProcent { get; set; }
    public decimal MaxHojningProcent { get; set; }
}
