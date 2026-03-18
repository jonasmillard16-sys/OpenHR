using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace RegionHR.Web.Services;

public class AuthService
{
    private readonly ProtectedSessionStorage _storage;

    public string? UserName { get; private set; }
    public string? Role { get; private set; }
    public bool IsLoggedIn => UserName != null;
    public bool IsInitialized { get; private set; }

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
            if (nameResult.Success) UserName = nameResult.Value;
            if (roleResult.Success) Role = roleResult.Value;
        }
        catch { /* First load, no stored values */ }
        IsInitialized = true;
    }

    public async Task LoginAsync(string userName, string role)
    {
        UserName = userName;
        Role = role;
        await _storage.SetAsync("auth_user", userName);
        await _storage.SetAsync("auth_role", role);
    }

    public async Task LogoutAsync()
    {
        UserName = null;
        Role = null;
        await _storage.DeleteAsync("auth_user");
        await _storage.DeleteAsync("auth_role");
    }

    public bool HasRole(params string[] roles) => Role != null && roles.Contains(Role);
    public bool IsAdmin => Role == "Admin";
    public bool IsHR => Role is "HR" or "Admin";
    public bool IsChef => Role is "Chef" or "HR" or "Admin";
}
