using System.Security.Claims;
using System.Text;
using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InventoryLab3.Services;

public class TokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public AuthResponse Create(User user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            NotBefore = DateTime.UtcNow,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        string token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AuthResponse(token, expires);
    }
}
