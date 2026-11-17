namespace InventoryLab3.Dtos;

public record StockBalance(int ProductId, string Sku, string Name, string Unit, int Quantity);

public record MovementRow(
    int Id, DateTime CreatedAt, string Product, string Type,
    string? From, string? To, int Quantity, string? Comment);

public record WarehouseTurnover(int WarehouseId, string Name, int TotalIn, int TotalOut);

public record TopProductRow(string Sku, string Name, int TotalMoved, int MovementsCount);
