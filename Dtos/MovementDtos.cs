using System.ComponentModel.DataAnnotations;

namespace InventoryLab3.Dtos;

public class MovementCreateDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public int? FromWarehouseId { get; set; }

    public int? ToWarehouseId { get; set; }

    [Range(1, 1_000_000)]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromWarehouseId == null && ToWarehouseId == null)
        {
            yield return new ValidationResult(
                "Потрібно вказати FromWarehouseId, ToWarehouseId або обидва",
                new[] { nameof(FromWarehouseId), nameof(ToWarehouseId) });
        }

        if (FromWarehouseId != null && FromWarehouseId == ToWarehouseId)
        {
            yield return new ValidationResult(
                "Склади відправлення і призначення збігаються",
                new[] { nameof(ToWarehouseId) });
        }
    }
}

public record MovementDto(
    int Id,
    int ProductId,
    string ProductName,
    string Type,
    int? FromWarehouseId,
    string? FromWarehouse,
    int? ToWarehouseId,
    string? ToWarehouse,
    int Quantity,
    int CreatedByUserId,
    DateTime CreatedAt,
    string? Comment);
