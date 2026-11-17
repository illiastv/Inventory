using InventoryLab3.Data;
using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<StockMovement>> GetMovementsWithRelated(int warehouseId)
    {
        var query = from m in _db.StockMovements.AsNoTracking()
                        .Include(nameof(StockMovement.Product))
                        .Include(nameof(StockMovement.FromWarehouse))
                        .Include(nameof(StockMovement.ToWarehouse))
                        .Include(nameof(StockMovement.CreatedByUser))
                    where m.FromWarehouseId == warehouseId || m.ToWarehouseId == warehouseId
                    orderby m.CreatedAt descending, m.Id descending
                    select m;

        return await query.ToListAsync();
    }

    public async Task<List<MovementRow>> GetMovementRows(DateTime fromUtc, DateTime toUtc)
    {
        var query = from m in _db.StockMovements.AsNoTracking()
                    where m.CreatedAt >= fromUtc && m.CreatedAt <= toUtc
                    orderby m.CreatedAt, m.Id
                    select new MovementRow(
                        m.Id,
                        m.CreatedAt,
                        m.Product.Name,
                        m.FromWarehouseId == null ? "Прихід"
                            : m.ToWarehouseId == null ? "Витрата" : "Переміщення",
                        m.FromWarehouse != null ? m.FromWarehouse.Name : null,
                        m.ToWarehouse != null ? m.ToWarehouse.Name : null,
                        m.Quantity,
                        m.Comment);

        return await query.ToListAsync();
    }

    public async Task<List<WarehouseTurnover>> GetTurnoverPerWarehouse()
    {
        var query = from w in _db.Warehouses.AsNoTracking()
                    orderby w.Name
                    select new WarehouseTurnover(
                        w.Id,
                        w.Name,
                        (from m in w.IncomingMovements select m.Quantity).Sum(),
                        (from m in w.OutgoingMovements select m.Quantity).Sum());

        return await query.ToListAsync();
    }

    public async Task<List<TopProductRow>> GetTopProducts(int take = 5)
    {
        var totals = from m in _db.StockMovements
                     group m by m.ProductId into g
                     select new
                     {
                         ProductId = g.Key,
                         Total = (from x in g select x.Quantity).Sum(),
                         Count = g.Count()
                     };

        var top = from t in totals
                  orderby t.Total descending
                  select t;

        var rows = await top.Take(take).ToListAsync();

        var result = new List<TopProductRow>();
        foreach (var row in rows)
        {
            var product = await _db.Products.FindAsync(row.ProductId);
            result.Add(new TopProductRow(product!.Sku, product.Name, row.Total, row.Count));
        }
        return result;
    }
}
