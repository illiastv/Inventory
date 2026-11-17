using InventoryLab3.Data;
using InventoryLab3.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Services;

public class WarehouseService
{
    private readonly AppDbContext _db;

    public WarehouseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Warehouse> Create(string name, string city, int ownerId)
    {
        var owner = await _db.Users.FindAsync(ownerId);
        if (owner == null)
            throw new ArgumentException($"Власника {ownerId} не знайдено");

        var warehouse = new Warehouse { Name = name, City = city, OwnerId = ownerId };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> Get(int id)
    {
        var warehouse = await _db.Warehouses.FindAsync(id);
        if (warehouse == null)
            throw new NotFoundException($"Склад {id} не знайдено");

        return warehouse;
    }

    public async Task<Warehouse> GetForUser(int id, int userId, bool isAdmin)
    {
        var warehouse = await Get(id);
        if (!isAdmin && warehouse.OwnerId != userId)
            throw new ForbiddenException("Немає доступу до цього складу");

        return warehouse;
    }

    public async Task<List<Warehouse>> Search(string? text, string? city, int? ownerId, string sortBy, bool desc)
    {
        sortBy = sortBy.ToLowerInvariant();
        if (sortBy != "name" && sortBy != "city")
            throw new ArgumentException("sortBy може бути: name або city");

        IQueryable<Warehouse> query = _db.Warehouses;

        if (!string.IsNullOrWhiteSpace(text))
        {
            string s = text.Trim().ToLower();
            query = from w in query
                    where w.Name.ToLower().Contains(s)
                    select w;
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            string c = city.Trim();
            query = from w in query
                    where w.City == c
                    select w;
        }

        if (ownerId != null)
        {
            query = from w in query
                    where w.OwnerId == ownerId
                    select w;
        }

        if (sortBy == "city" && desc)
            query = from w in query orderby w.City descending, w.Name select w;
        else if (sortBy == "city")
            query = from w in query orderby w.City, w.Name select w;
        else if (desc)
            query = from w in query orderby w.Name descending select w;
        else
            query = from w in query orderby w.Name select w;

        return await query.ToListAsync();
    }

    public async Task<Warehouse> Update(int id, string name, string city)
    {
        var warehouse = await Get(id);

        warehouse.Name = name;
        warehouse.City = city;
        await _db.SaveChangesAsync();
        return warehouse;
    }

    public async Task Delete(int id)
    {
        var warehouse = await Get(id);

        var movements = from m in _db.StockMovements
                        where m.FromWarehouseId == id || m.ToWarehouseId == id
                        select m;
        if (await movements.AnyAsync())
            throw new ConflictException("Неможливо видалити склад, по якому є рухи");

        _db.Warehouses.Remove(warehouse);
        await _db.SaveChangesAsync();
    }
}
