namespace RegionHR.SharedKernel.Domain;

public readonly record struct VendorId(Guid Value)
{
    public static VendorId New() => new(Guid.NewGuid());
    public static VendorId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct StaffingRequestId(Guid Value)
{
    public static StaffingRequestId New() => new(Guid.NewGuid());
    public static StaffingRequestId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct FrameworkAgreementId(Guid Value)
{
    public static FrameworkAgreementId New() => new(Guid.NewGuid());
    public static FrameworkAgreementId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
