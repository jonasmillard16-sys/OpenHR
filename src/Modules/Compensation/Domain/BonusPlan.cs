using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Bonusplan. Hanterar individuella, grupp- och foretagsbonusar.
/// </summary>
public sealed class BonusPlan : AggregateRoot<BonusPlanId>
{
    public string Namn { get; private set; } = string.Empty;
    public BonusTyp Typ { get; private set; }
    public string? BerakningsModell { get; private set; }  // JSON
    public string? UtbetalningsTidpunkt { get; private set; }
    public BonusPlanStatus Status { get; private set; }

    private readonly List<BonusTarget> _targets = [];
    public IReadOnlyList<BonusTarget> Targets => _targets.AsReadOnly();

    private BonusPlan() { }

    public static BonusPlan Skapa(string namn, BonusTyp typ, string? utbetalningsTidpunkt = null)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn maste anges", nameof(namn));

        return new BonusPlan
        {
            Id = BonusPlanId.New(),
            Namn = namn,
            Typ = typ,
            UtbetalningsTidpunkt = utbetalningsTidpunkt,
            Status = BonusPlanStatus.Draft
        };
    }

    public void SattBerakningsModell(string modellJson)
    {
        BerakningsModell = modellJson;
    }

    public void Aktivera()
    {
        if (Status != BonusPlanStatus.Draft)
            throw new InvalidOperationException("Kan bara aktivera en plan med status Draft");
        Status = BonusPlanStatus.Active;
    }

    public void Stang()
    {
        if (Status != BonusPlanStatus.Active)
            throw new InvalidOperationException("Kan bara stanga en aktiv plan");
        Status = BonusPlanStatus.Closed;
    }

    public void LaggTillTarget(BonusTarget target)
    {
        target.BonusPlanId = Id;
        _targets.Add(target);
    }
}

public enum BonusTyp
{
    Individual,
    Grupp,
    Foretag
}

public enum BonusPlanStatus
{
    Draft,
    Active,
    Closed
}
