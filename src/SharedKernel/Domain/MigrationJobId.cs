namespace RegionHR.SharedKernel.Domain;

public readonly record struct MigrationJobId(Guid Value)
{
    public static MigrationJobId New() => new(Guid.NewGuid());
    public static MigrationJobId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
