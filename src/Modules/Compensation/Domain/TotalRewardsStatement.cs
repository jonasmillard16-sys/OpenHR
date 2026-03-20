using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Total Rewards Statement — sammanstallning av alla ersattningar for en anstalld.
/// </summary>
public sealed class TotalRewardsStatement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmployeeId AnstallId { get; set; }
    public int Ar { get; set; }
    public decimal GrundLon { get; set; }
    public decimal Tillagg { get; set; }
    public decimal Pension { get; set; }
    public decimal Forsakringar { get; set; }
    public decimal Formaner { get; set; }
    public decimal AGAvgifter { get; set; }
    public decimal TotalKompensation { get; set; }
    public DateTime GenereradVid { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Genererar ett nytt TotalRewardsStatement baserat pa loneuppgifter.
    /// </summary>
    public static TotalRewardsStatement Generera(
        EmployeeId anstallId, int ar,
        decimal grundLon, decimal tillagg, decimal pension,
        decimal forsakringar, decimal formaner, decimal agAvgifter)
    {
        return new TotalRewardsStatement
        {
            AnstallId = anstallId,
            Ar = ar,
            GrundLon = grundLon,
            Tillagg = tillagg,
            Pension = pension,
            Forsakringar = forsakringar,
            Formaner = formaner,
            AGAvgifter = agAvgifter,
            TotalKompensation = grundLon + tillagg + pension + forsakringar + formaner + agAvgifter,
            GenereradVid = DateTime.UtcNow
        };
    }
}
