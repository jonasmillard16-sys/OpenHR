namespace RegionHR.Performance.Domain;

/// <summary>
/// Chefs-styrkort med aggregerade nyckeltal för en period.
/// </summary>
public sealed class ManagerScorecard
{
    public Guid Id { get; private set; }
    public Guid ChefId { get; private set; }
    public string Period { get; private set; } = default!; // e.g. "2026-Q1"
    public int SpanOfControl { get; private set; }
    public decimal TeamOmsattning { get; private set; } // % turnover
    public decimal EngagementDelta { get; private set; } // change in engagement score
    public decimal UtvecklingsplanFardiggrad { get; private set; } // % development plan completion
    public decimal MedelTidMellanOneonone { get; private set; } // average days between 1:1s
    public DateTime GenereradVid { get; private set; }

    private ManagerScorecard() { } // EF Core

    public static ManagerScorecard Generera(
        Guid chefId,
        string period,
        int spanOfControl,
        decimal teamOmsattning,
        decimal engagementDelta,
        decimal utvecklingsplanFardiggrad,
        decimal medelTidMellanOneonone)
    {
        if (chefId == Guid.Empty) throw new ArgumentException("ChefId krävs.", nameof(chefId));
        ArgumentException.ThrowIfNullOrWhiteSpace(period);

        return new ManagerScorecard
        {
            Id = Guid.NewGuid(),
            ChefId = chefId,
            Period = period,
            SpanOfControl = spanOfControl,
            TeamOmsattning = teamOmsattning,
            EngagementDelta = engagementDelta,
            UtvecklingsplanFardiggrad = utvecklingsplanFardiggrad,
            MedelTidMellanOneonone = medelTidMellanOneonone,
            GenereradVid = DateTime.UtcNow
        };
    }
}
