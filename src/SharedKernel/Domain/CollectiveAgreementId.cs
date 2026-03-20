namespace RegionHR.SharedKernel.Domain;

public readonly record struct CollectiveAgreementId(Guid Value)
{
    public static CollectiveAgreementId New() => new(Guid.NewGuid());
    public static CollectiveAgreementId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
