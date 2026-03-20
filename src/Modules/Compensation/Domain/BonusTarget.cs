using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Bonusmal kopplat till en bonusplan. KPI-baserat med vikt, troskel och tak.
/// </summary>
public sealed class BonusTarget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public BonusPlanId BonusPlanId { get; set; }
    public Guid? AnstallId { get; set; }
    public Guid? GruppId { get; set; }
    public string MalKPI { get; set; } = string.Empty;
    public decimal Vikt { get; set; }
    public decimal Troskel { get; set; }
    public decimal Tak { get; set; }

    private readonly List<BonusOutcome> _utfall = [];
    public IReadOnlyList<BonusOutcome> Utfall => _utfall.AsReadOnly();

    public void LaggTillUtfall(BonusOutcome utfall)
    {
        utfall.BonusTargetId = Id;
        _utfall.Add(utfall);
    }
}
