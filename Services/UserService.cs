using InventoryLab3.Data;
using InventoryLab3.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> Create(string email, string passwordHash, string role = Roles.User)
    {
        var existing = await GetByEmail(email);
        if (existing != null)
            throw new ConflictException($"Користувач '{email}' вже існує");

        var user = new User { Email = email, PasswordHash = passwordHash, Role = role };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByEmail(string email)
    {
        var query = from u in _db.Users
                    where u.Email == email
                    select u;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetOrCreate(string email, string passwordHash, string role = Roles.User)
    {
        var existing = await GetByEmail(email);
        if (existing != null) return existing;

        return await Create(email, passwordHash, role);
    }

    public async Task<User> Get(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            throw new NotFoundException($"Користувача {id} не знайдено");

        return user;
    }

    public async Task<List<User>> GetAll()
    {
        var query = from u in _db.Users
                    orderby u.Id
                    select u;

        return await query.ToListAsync();
    }

    public async Task<User> SetRole(int id, string role)
    {
        if (role != Roles.User && role != Roles.Admin)
            throw new ArgumentException("Роль може бути: User або Admin");

        var user = await Get(id);
        user.Role = role;
        await _db.SaveChangesAsync();
        return user;
    }
}
