using InventoryLab3.Data;
using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Services;

public class StockService
{
    private readonly AppDbContext _db;

    public StockService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StockMovement> Receive(int productId, int toWarehouseId, int qty, int userId, string? comment = null)
    {
        return await WriteMovement(productId, null, toWarehouseId, qty, userId, comment);
    }

    public async Task<StockMovement> Issue(int productId, int fromWarehouseId, int qty, int userId, string? comment = null)
    {
        return await WriteMovement(productId, fromWarehouseId, null, qty, userId, comment);
    }

    public async Task<StockMovement> Transfer(int productId, int fromWarehouseId, int toWarehouseId, int qty, int userId, string? comment = null)
    {
        if (fromWarehouseId == toWarehouseId)
            throw new ArgumentException("Склади відправлення і призначення збігаються");

        return await WriteMovement(productId, fromWarehouseId, toWarehouseId, qty, userId, comment);
    }

    public async Task<StockMovement> Get(int id)
    {
        var query = from m in MovementsWithRelated()
                    where m.Id == id
                    select m;

        var movement = await query.FirstOrDefaultAsync();
        if (movement == null)
            throw new NotFoundException($"Рух {id} не знайдено");

        return movement;
    }

    public async Task<StockMovement> GetForUser(int id, int userId, bool isAdmin)
    {
        var movement = await Get(id);

        if (!isAdmin)
        {
            bool fromOwn = movement.FromWarehouse != null && movement.FromWarehouse.OwnerId == userId;
            bool toOwn = movement.ToWarehouse != null && movement.ToWarehouse.OwnerId == userId;
            if (!fromOwn && !toOwn)
                throw new ForbiddenException("Немає доступу до цього руху");
        }

        return movement;
    }

    public async Task<List<StockMovement>> Search(int? productId, int? warehouseId, string? type, string sortBy, bool desc, int? visibleToUserId = null)
    {
        sortBy = sortBy.ToLowerInvariant();
        if (sortBy != "date" && sortBy != "quantity")
            throw new ArgumentException("sortBy може бути: date або quantity");

        IQueryable<StockMovement> query = MovementsWithRelated();

        if (productId != null)
        {
            query = from m in query
                    where m.ProductId == productId
                    select m;
        }

        if (warehouseId != null)
        {
            query = from m in query
                    where m.FromWarehouseId == warehouseId || m.ToWarehouseId == warehouseId
                    select m;
        }

        if (visibleToUserId != null)
        {
            query = from m in query
                    where (m.FromWarehouse != null && m.FromWarehouse.OwnerId == visibleToUserId)
                       || (m.ToWarehouse != null && m.ToWarehouse.OwnerId == visibleToUserId)
                    select m;
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            string t = type.Trim().ToLowerInvariant();

            if (t == "receive")
                query = from m in query where m.FromWarehouseId == null select m;
            else if (t == "issue")
                query = from m in query where m.ToWarehouseId == null select m;
            else if (t == "transfer")
                query = from m in query where m.FromWarehouseId != null && m.ToWarehouseId != null select m;
            else
                throw new ArgumentException("type може бути: receive, issue або transfer");
        }

        if (sortBy == "quantity" && desc)
            query = from m in query orderby m.Quantity descending, m.Id descending select m;
        else if (sortBy == "quantity")
            query = from m in query orderby m.Quantity, m.Id select m;
        else if (desc)
            query = from m in query orderby m.CreatedAt descending, m.Id descending select m;
        else
            query = from m in query orderby m.CreatedAt, m.Id select m;

        return await query.ToListAsync();
    }

    public async Task<int> GetStock(int productId, int warehouseId)
    {
        var incoming = from m in _db.StockMovements
                       where m.ProductId == productId && m.ToWarehouseId == warehouseId
                       select m.Quantity;

        var outgoing = from m in _db.StockMovements
                       where m.ProductId == productId && m.FromWarehouseId == warehouseId
                       select m.Quantity;

        int totalIn = await incoming.SumAsync();
        int totalOut = await outgoing.SumAsync();
        return totalIn - totalOut;
    }

    public async Task<List<StockBalance>> GetBalances(int warehouseId)
    {
        var incoming = from m in _db.StockMovements
                       where m.ToWarehouseId == warehouseId
                       group m by m.ProductId into g
                       select new { ProductId = g.Key, Total = (from x in g select x.Quantity).Sum() };

        var outgoing = from m in _db.StockMovements
                       where m.FromWarehouseId == warehouseId
                       group m by m.ProductId into g
                       select new { ProductId = g.Key, Total = (from x in g select x.Quantity).Sum() };

        var balances = new SortedDictionary<int, int>();

        foreach (var row in await incoming.ToListAsync())
        {
            balances[row.ProductId] = row.Total;
        }

        foreach (var row in await outgoing.ToListAsync())
        {
            balances.TryGetValue(row.ProductId, out int current);
            balances[row.ProductId] = current - row.Total;
        }

        var result = new List<StockBalance>();
        foreach (var pair in balances)
        {
            if (pair.Value == 0) continue;

            var product = await _db.Products.FindAsync(pair.Key);
            result.Add(new StockBalance(product!.Id, product.Sku, product.Name, product.Unit, pair.Value));
        }
        return result;
    }

    private IQueryable<StockMovement> MovementsWithRelated()
    {
        return _db.StockMovements.AsNoTracking()
            .Include(nameof(StockMovement.Product))
            .Include(nameof(StockMovement.FromWarehouse))
            .Include(nameof(StockMovement.ToWarehouse));
    }

    private async Task<StockMovement> WriteMovement(
        int productId, int? fromId, int? toId, int qty, int userId, string? comment)
    {
        if (qty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qty), "Кількість має бути > 0");

        if (await _db.Products.FindAsync(productId) == null)
            throw new ArgumentException($"Товар {productId} не знайдено");
        if (await _db.Users.FindAsync(userId) == null)
            throw new ArgumentException($"Користувача {userId} не знайдено");
        if (fromId != null && await _db.Warehouses.FindAsync(fromId.Value) == null)
            throw new ArgumentException($"Склад {fromId} не знайдено");
        if (toId != null && await _db.Warehouses.FindAsync(toId.Value) == null)
            throw new ArgumentException($"Склад {toId} не знайдено");

        await using var transaction = await _db.Database.BeginTransactionAsync();

        if (fromId != null)
        {
            int available = await GetStock(productId, fromId.Value);
            if (available < qty)
                throw new InsufficientStockException(productId, fromId.Value, available, qty);
        }

        var movement = new StockMovement
        {
            ProductId = productId,
            FromWarehouseId = fromId,
            ToWarehouseId = toId,
            Quantity = qty,
            CreatedByUserId = userId,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return movement;
    }
}
