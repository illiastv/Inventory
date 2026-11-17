using InventoryLab3.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Data;

public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
