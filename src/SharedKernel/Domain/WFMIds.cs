namespace RegionHR.SharedKernel.Domain;

public readonly record struct DemandForecastId(Guid Value)
{
    public static DemandForecastId New() => new(Guid.NewGuid());
    public static DemandForecastId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct SchedulingRunId(Guid Value)
{
    public static SchedulingRunId New() => new(Guid.NewGuid());
    public static SchedulingRunId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
