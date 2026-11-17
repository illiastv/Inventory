using System.ComponentModel.DataAnnotations;

namespace InventoryLab3.Dtos;

public class ProductCreateDto
{
    [Required, StringLength(64)]
    public string Sku { get; set; } = "";

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(16)]
    public string Unit { get; set; } = "pcs";
}

public class ProductUpdateDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(16)]
    public string Unit { get; set; } = "pcs";
}

public record ProductDto(int Id, string Sku, string Name, string Unit);
