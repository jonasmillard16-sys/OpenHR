using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace RegionHR.Web.Services;

public class AuthService
{
    private readonly ProtectedSessionStorage _storage;

    public string? UserName { get; private set; }
    public string? Role { get; private set; }
    /// <summary>
    /// EmployeeId för den inloggade användaren. Null om inloggad som Admin
    /// (ingen Employee-koppling) eller om ingen matchning hittades.
    /// Mappas via exakt namnmatchning mot Employee-tabellen vid login.
    /// Detta är en demo-auth-begränsning — inte en riktig identitetslösning.
    /// </summary>
    public Guid? EmployeeId { get; private set; }
    public bool IsLoggedIn => UserName != null;
    public bool IsInitialized { get; private set; }
    public bool IsDarkMode { get; private set; }

    public AuthService(ProtectedSessionStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;
        try
        {
            var nameResult = await _storage.GetAsync<string>("auth_user");
            var roleResult = await _storage.GetAsync<string>("auth_role");
            var empIdResult = await _storage.GetAsync<string>("auth_employee_id");
            var darkResult = await _storage.GetAsync<bool>("dark_mode");
            if (nameResult.Success) UserName = nameResult.Value;
            if (roleResult.Success) Role = roleResult.Value;
            if (empIdResult.Success && Guid.TryParse(empIdResult.Value, out var empId)) EmployeeId = empId;
            if (darkResult.Success) IsDarkMode = darkResult.Value;
        }
        catch { /* First load, no stored values */ }
        IsInitialized = true;
    }

    public async Task LoginAsync(string userName, string role, Guid? employeeId = null)
    {
        UserName = userName;
        Role = role;
        EmployeeId = employeeId;
        await _storage.SetAsync("auth_user", userName);
        await _storage.SetAsync("auth_role", role);
        if (employeeId.HasValue)
            await _storage.SetAsync("auth_employee_id", employeeId.Value.ToString());
        else
            await _storage.DeleteAsync("auth_employee_id");
    }

    public async Task LogoutAsync()
    {
        UserName = null;
        Role = null;
        EmployeeId = null;
        await _storage.DeleteAsync("auth_user");
        await _storage.DeleteAsync("auth_role");
        await _storage.DeleteAsync("auth_employee_id");
    }

    public async Task SetDarkModeAsync(bool value)
    {
        IsDarkMode = value;
        await _storage.SetAsync("dark_mode", value);
    }

    public bool HasRole(params string[] roles) => Role != null && roles.Contains(Role);
    public bool IsAdmin => Role == "Admin";
    public bool IsHR => Role is "HR" or "Admin";
    public bool IsChef => Role is "Chef" or "HR" or "Admin";
}
