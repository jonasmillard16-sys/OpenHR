namespace RegionHR.SharedKernel.Domain;

public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId New() => new(Guid.NewGuid());
    public static OrganizationId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct EmployeeId(Guid Value)
{
    public static EmployeeId New() => new(Guid.NewGuid());
    public static EmployeeId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct EmploymentId(Guid Value)
{
    public static EmploymentId New() => new(Guid.NewGuid());
    public static EmploymentId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct CaseId(Guid Value)
{
    public static CaseId New() => new(Guid.NewGuid());
    public static CaseId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct PayrollRunId(Guid Value)
{
    public static PayrollRunId New() => new(Guid.NewGuid());
    public static PayrollRunId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct ScheduleId(Guid Value)
{
    public static ScheduleId New() => new(Guid.NewGuid());
    public static ScheduleId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct StaffingTemplateId(Guid Value)
{
    public static StaffingTemplateId New() => new(Guid.NewGuid());
    public static StaffingTemplateId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct ShiftSwapId(Guid Value)
{
    public static ShiftSwapId New() => new(Guid.NewGuid());
    public static ShiftSwapId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
