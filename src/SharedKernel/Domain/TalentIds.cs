namespace RegionHR.SharedKernel.Domain;

public readonly record struct CareerPathId(Guid Value)
{
    public static CareerPathId New() => new(Guid.NewGuid());
    public static CareerPathId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct DevelopmentPlanId(Guid Value)
{
    public static DevelopmentPlanId New() => new(Guid.NewGuid());
    public static DevelopmentPlanId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct InternalOpportunityId(Guid Value)
{
    public static InternalOpportunityId New() => new(Guid.NewGuid());
    public static InternalOpportunityId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
