namespace RegionHR.Api.Auth;

/// <summary>
/// Systemets roller (RBAC).
/// Mappas från Azure AD-grupper eller Supabase app_metadata.
/// </summary>
public static class Roles
{
    public const string Anstalld = "Anstalld";
    public const string Chef = "Chef";
    public const string HRAdmin = "HR-admin";
    public const string HRSpecialist = "HR-specialist";
    public const string Loneadmin = "Loneadmin";
    public const string Systemadmin = "Systemadmin";
    public const string FackligRepresentant = "FackligRepresentant";

    public static readonly string[] AllRoles =
    [
        Anstalld, Chef, HRAdmin, HRSpecialist, Loneadmin, Systemadmin, FackligRepresentant
    ];
}
