using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Compensation.Domain;

/// <summary>
/// Kompensationsplan. Styr budget och riktlinjer for en lonerevision eller bonusperiod.
/// </summary>
public sealed class CompensationPlan : AggregateRoot<CompensationPlanId>
{
    public string Namn { get; private set; } = string.Empty;
    public DateOnly GiltigFran { get; private set; }
    public DateOnly GiltigTill { get; private set; }
    public decimal TotalBudget { get; private set; }
    public CompensationPlanStatus Status { get; private set; }

    private readonly List<CompensationBudget> _budgetar = [];
    public IReadOnlyList<CompensationBudget> Budgetar => _budgetar.AsReadOnly();

    private readonly List<CompensationGuideline> _riktlinjer = [];
    public IReadOnlyList<CompensationGuideline> Riktlinjer => _riktlinjer.AsReadOnly();

    private CompensationPlan() { }

    public static CompensationPlan Skapa(string namn, DateOnly giltigFran, DateOnly giltigTill, decimal totalBudget)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn maste anges", nameof(namn));
        if (giltigTill <= giltigFran)
            throw new ArgumentException("GiltigTill maste vara efter GiltigFran");
        if (totalBudget < 0)
            throw new ArgumentException("Budget kan inte vara negativ", nameof(totalBudget));

        return new CompensationPlan
        {
            Id = CompensationPlanId.New(),
            Namn = namn,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill,
            TotalBudget = totalBudget,
            Status = CompensationPlanStatus.Draft
        };
    }

    public void Aktivera()
    {
        if (Status != CompensationPlanStatus.Draft)
            throw new InvalidOperationException("Kan bara aktivera en plan med status Draft");
        Status = CompensationPlanStatus.Active;
    }

    public void Stang()
    {
        if (Status != CompensationPlanStatus.Active)
            throw new InvalidOperationException("Kan bara stanga en aktiv plan");
        Status = CompensationPlanStatus.Closed;
    }

    public void LaggTillBudget(CompensationBudget budget)
    {
        _budgetar.Add(budget);
    }

    public void LaggTillRiktlinje(CompensationGuideline riktlinje)
    {
        _riktlinjer.Add(riktlinje);
    }
}

public enum CompensationPlanStatus
{
    Draft,
    Active,
    Closed
}
