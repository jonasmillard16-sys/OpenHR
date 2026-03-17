using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RegionHR.Api.Auth;

/// <summary>
/// Genererar JWT-tokens för lokal utveckling och testning.
/// ENBART för dev-miljö -- aldrig i produktion.
/// </summary>
public static class DevTokenGenerator
{
    public static string GenerateToken(
        string userId,
        string name,
        string email,
        string role,
        string secret,
        string issuer = "https://regionhr.local",
        string audience = "regionhr-api",
        int expirationMinutes = 480)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role), // Supabase-kompatibel
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
