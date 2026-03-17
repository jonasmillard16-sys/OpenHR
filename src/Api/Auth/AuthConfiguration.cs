using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RegionHR.Api.Auth;

/// <summary>
/// Konfiguration för autentisering och auktorisering.
/// Stöder Azure AD (Entra ID) via JWT Bearer och Supabase Auth för lokal dev.
/// </summary>
public static class AuthConfiguration
{
    public static IServiceCollection AddRegionHRAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"] ?? "https://regionhr.local",
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"] ?? "regionhr-api",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Secret"] ?? "dev-secret-key-min-32-characters-long!!")),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                // Stöd för Supabase JWT i dev
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Extrahera roller från Supabase JWT "app_metadata.role" eller Azure AD "roles"
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        var roleClaim = identity?.FindFirst("role") ?? identity?.FindFirst("app_metadata.role");
                        if (roleClaim != null && !identity!.HasClaim(ClaimTypes.Role, roleClaim.Value))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            // Rollbaserade policies
            .AddPolicy("Anstalld", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("Chef", policy => policy.RequireRole("Chef", "HR-admin", "Systemadmin"))
            .AddPolicy("HR", policy => policy.RequireRole("HR-admin", "HR-specialist", "Systemadmin"))
            .AddPolicy("Loneadmin", policy => policy.RequireRole("Loneadmin", "Systemadmin"))
            .AddPolicy("Systemadmin", policy => policy.RequireRole("Systemadmin"))
            .AddPolicy("FackligRepresentant", policy => policy.RequireRole("FackligRepresentant", "HR-admin", "Systemadmin"))
            // Kombinerade policies
            .AddPolicy("ChefEllerHR", policy => policy.RequireRole("Chef", "HR-admin", "HR-specialist", "Systemadmin"))
            .AddPolicy("LonOchHR", policy => policy.RequireRole("Loneadmin", "HR-admin", "Systemadmin"));

        return services;
    }
}
