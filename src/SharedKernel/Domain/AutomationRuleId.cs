namespace RegionHR.SharedKernel.Domain;

public readonly record struct AutomationRuleId(Guid Value)
{
    public static AutomationRuleId New() => new(Guid.NewGuid());
    public static AutomationRuleId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct AutomationCategoryId(Guid Value)
{
    public static AutomationCategoryId New() => new(Guid.NewGuid());
    public static AutomationCategoryId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
