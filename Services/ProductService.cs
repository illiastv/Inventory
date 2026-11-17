using InventoryLab3.Data;
using InventoryLab3.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product> Create(string sku, string name, string unit)
    {
        var existing = await GetBySku(sku);
        if (existing != null)
            throw new ConflictException($"Товар з SKU '{sku}' вже існує");

        var product = new Product { Sku = sku, Name = name, Unit = unit };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> Get(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            throw new NotFoundException($"Товар {id} не знайдено");

        return product;
    }

    public async Task<Product?> GetBySku(string sku)
    {
        var query = from p in _db.Products
                    where p.Sku == sku
                    select p;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<Product>> Search(string? text, string? unit, string sortBy, bool desc)
    {
        sortBy = sortBy.ToLowerInvariant();
        if (sortBy != "name" && sortBy != "sku")
            throw new ArgumentException("sortBy може бути: name або sku");

        IQueryable<Product> query = _db.Products;

        if (!string.IsNullOrWhiteSpace(text))
        {
            string s = text.Trim().ToLower();
            query = from p in query
                    where p.Name.ToLower().Contains(s) || p.Sku.ToLower().Contains(s)
                    select p;
        }

        if (!string.IsNullOrWhiteSpace(unit))
        {
            string u = unit.Trim();
            query = from p in query
                    where p.Unit == u
                    select p;
        }

        if (sortBy == "sku" && desc)
            query = from p in query orderby p.Sku descending select p;
        else if (sortBy == "sku")
            query = from p in query orderby p.Sku select p;
        else if (desc)
            query = from p in query orderby p.Name descending select p;
        else
            query = from p in query orderby p.Name select p;

        return await query.ToListAsync();
    }

    public async Task<Product> Update(int id, string name, string unit)
    {
        var product = await Get(id);

        product.Name = name;
        product.Unit = unit;
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task Delete(int id)
    {
        var product = await Get(id);

        var movements = from m in _db.StockMovements
                        where m.ProductId == id
                        select m;
        if (await movements.AnyAsync())
            throw new ConflictException("Неможливо видалити товар, по якому є рухи");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}
