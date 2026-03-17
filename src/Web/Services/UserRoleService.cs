namespace RegionHR.Web.Services;

/// <summary>
/// Determines the current user's role for layout routing.
/// TODO: Replace with actual JWT claims reading when auth is connected.
/// </summary>
public class UserRoleService
{
    // For now, configurable via query string: ?role=anstalld|chef|admin
    // In production, this reads from the authenticated user's JWT claims.
    public UserRole CurrentRole { get; set; } = UserRole.Admin;
}

public enum UserRole
{
    Anstalld,
    Chef,
    Admin
}
