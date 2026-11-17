using InventoryLab3.Domain;

namespace InventoryLab3.Dtos;

public static class Mapping
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(product.Id, product.Sku, product.Name, product.Unit);
    }

    public static List<ProductDto> ToDtos(List<Product> products)
    {
        var result = new List<ProductDto>();
        foreach (var product in products)
        {
            result.Add(ToDto(product));
        }
        return result;
    }

    public static WarehouseDto ToDto(Warehouse warehouse)
    {
        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.City, warehouse.OwnerId);
    }

    public static List<WarehouseDto> ToDtos(List<Warehouse> warehouses)
    {
        var result = new List<WarehouseDto>();
        foreach (var warehouse in warehouses)
        {
            result.Add(ToDto(warehouse));
        }
        return result;
    }

    public static MovementDto ToDto(StockMovement movement)
    {
        string type;
        if (movement.FromWarehouseId == null)
            type = "Receive";
        else if (movement.ToWarehouseId == null)
            type = "Issue";
        else
            type = "Transfer";

        return new MovementDto(
            movement.Id,
            movement.ProductId,
            movement.Product.Name,
            type,
            movement.FromWarehouseId,
            movement.FromWarehouse?.Name,
            movement.ToWarehouseId,
            movement.ToWarehouse?.Name,
            movement.Quantity,
            movement.CreatedByUserId,
            movement.CreatedAt,
            movement.Comment);
    }

    public static List<MovementDto> ToDtos(List<StockMovement> movements)
    {
        var result = new List<MovementDto>();
        foreach (var movement in movements)
        {
            result.Add(ToDto(movement));
        }
        return result;
    }

    public static UserDto ToDto(User user)
    {
        return new UserDto(user.Id, user.Email, user.Role);
    }

    public static List<UserDto> ToDtos(List<User> users)
    {
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            result.Add(ToDto(user));
        }
        return result;
    }
}
