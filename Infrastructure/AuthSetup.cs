using System.Text;
using InventoryLab3.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace InventoryLab3.Infrastructure;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
}

public static class AuthSetup
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

        if (settings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key не задано або він коротший за 32 символи. " +
                "Задай через: dotnet user-secrets set \"Jwt:Key\" \"<довгий випадковий рядок>\"");
        }

        services.AddSingleton(settings);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));
        });

        return services;
    }
}
