namespace RegionHR.Compensation.Domain;

/// <summary>
/// Utfall for ett bonusmal. Hanterar berakning och godkannande.
/// </summary>
public sealed class BonusOutcome
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BonusTargetId { get; set; }
    public decimal UtfallVarde { get; set; }
    public decimal BeraknatBelopp { get; set; }
    public BonusOutcomeStatus Status { get; set; } = BonusOutcomeStatus.Pending;

    public void Berakna(decimal belopp)
    {
        if (Status != BonusOutcomeStatus.Pending)
            throw new InvalidOperationException("Kan bara berakna utfall med status Pending");
        BeraknatBelopp = belopp;
        Status = BonusOutcomeStatus.Calculated;
    }

    public void Godkann()
    {
        if (Status != BonusOutcomeStatus.Calculated)
            throw new InvalidOperationException("Kan bara godkanna utfall med status Calculated");
        Status = BonusOutcomeStatus.Approved;
    }

    public void MarkeraSomUtbetald()
    {
        if (Status != BonusOutcomeStatus.Approved)
            throw new InvalidOperationException("Kan bara markera som utbetald med status Approved");
        Status = BonusOutcomeStatus.Paid;
    }
}

public enum BonusOutcomeStatus
{
    Pending,
    Calculated,
    Approved,
    Paid
}
