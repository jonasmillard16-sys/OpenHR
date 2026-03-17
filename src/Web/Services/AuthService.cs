namespace RegionHR.Web.Services;

public class AuthService
{
    public string? UserName { get; private set; }
    public string? Role { get; private set; } // Anställd, Chef, HR, Admin
    public bool IsLoggedIn => UserName != null;

    public void Login(string userName, string role) { UserName = userName; Role = role; }
    public void Logout() { UserName = null; Role = null; }

    public bool HasRole(params string[] roles) => Role != null && roles.Contains(Role);
    public bool IsAdmin => Role == "Admin";
    public bool IsHR => Role is "HR" or "Admin";
    public bool IsChef => Role is "Chef" or "HR" or "Admin";
}
