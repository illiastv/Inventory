using System.ComponentModel.DataAnnotations;

namespace InventoryLab3.Dtos;

public class WarehouseCreateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(100)]
    public string City { get; set; } = "";
}

public class WarehouseUpdateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(100)]
    public string City { get; set; } = "";
}

public record WarehouseDto(int Id, string Name, string City, int OwnerId);
