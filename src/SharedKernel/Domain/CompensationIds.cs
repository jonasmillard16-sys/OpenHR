namespace RegionHR.SharedKernel.Domain;

public readonly record struct CompensationPlanId(Guid Value)
{
    public static CompensationPlanId New() => new(Guid.NewGuid());
    public static CompensationPlanId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct BonusPlanId(Guid Value)
{
    public static BonusPlanId New() => new(Guid.NewGuid());
    public static BonusPlanId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
