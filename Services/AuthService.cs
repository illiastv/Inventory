using BCryptNet = BCrypt.Net.BCrypt;
using InventoryLab3.Domain;
using InventoryLab3.Dtos;

namespace InventoryLab3.Services;

public class AuthService
{
    private static readonly string DummyHash = BCryptNet.HashPassword("dummy-password-for-timing");

    private readonly UserService _users;
    private readonly TokenService _tokens;

    public AuthService(UserService users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<User> Register(string email, string password)
    {
        string hash = BCryptNet.HashPassword(password);
        return await _users.Create(Normalize(email), hash, Roles.User);
    }

    public async Task<AuthResponse> Login(string email, string password)
    {
        var user = await _users.GetByEmail(Normalize(email));

        string hash = user != null ? user.PasswordHash : DummyHash;
        bool passwordOk = BCryptNet.Verify(password, hash);

        if (user == null || !passwordOk)
            throw new UnauthorizedException("Невірний email або пароль");

        return _tokens.Create(user);
    }

    public async Task SeedAdmin(string email, string password)
    {
        var existing = await _users.GetByEmail(Normalize(email));
        if (existing != null) return;

        await _users.Create(Normalize(email), BCryptNet.HashPassword(password), Roles.Admin);
    }

    private static string Normalize(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
