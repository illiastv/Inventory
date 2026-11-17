using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InventoryLab3.Domain;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = "";

    [Required, MaxLength(512)]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(32)]
    public string Role { get; set; } = "User";

    public List<Warehouse> Warehouses { get; set; } = new();
}

public class Warehouse
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(100)]
    public string City { get; set; } = "";

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    [InverseProperty(nameof(StockMovement.ToWarehouse))]
    public List<StockMovement> IncomingMovements { get; set; } = new();

    [InverseProperty(nameof(StockMovement.FromWarehouse))]
    public List<StockMovement> OutgoingMovements { get; set; } = new();
}

[Index(nameof(Sku), IsUnique = true)]
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Sku { get; set; } = "";

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(16)]
    public string Unit { get; set; } = "pcs";

    public List<StockMovement> Movements { get; set; } = new();
}

[Index(nameof(CreatedAt))]
public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? FromWarehouseId { get; set; }
    [ForeignKey(nameof(FromWarehouseId))]
    [InverseProperty(nameof(Warehouse.OutgoingMovements))]
    public Warehouse? FromWarehouse { get; set; }

    public int? ToWarehouseId { get; set; }
    [ForeignKey(nameof(ToWarehouseId))]
    [InverseProperty(nameof(Warehouse.IncomingMovements))]
    public Warehouse? ToWarehouse { get; set; }

    public int Quantity { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Comment { get; set; }
}

