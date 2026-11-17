using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public class StockMovementsController : ControllerBase
{
    private readonly StockService _stock;
    private readonly WarehouseService _warehouses;

    public StockMovementsController(StockService stock, WarehouseService warehouses)
    {
        _stock = stock;
        _warehouses = warehouses;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovementDto>>> GetAll(
        [FromQuery] int? productId,
        [FromQuery] int? warehouseId,
        [FromQuery] string? type,
        [FromQuery] string sortBy = "date",
        [FromQuery] bool desc = true)
    {
        int userId = User.GetUserId();
        bool isAdmin = User.IsAdmin();

        if (warehouseId != null)
            await _warehouses.GetForUser(warehouseId.Value, userId, isAdmin);

        int? visibleTo = isAdmin ? null : userId;
        var movements = await _stock.Search(productId, warehouseId, type, sortBy, desc, visibleTo);
        return Ok(Mapping.ToDtos(movements));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovementDto>> GetById(int id)
    {
        var movement = await _stock.GetForUser(id, User.GetUserId(), User.IsAdmin());
        return Ok(Mapping.ToDto(movement));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<MovementDto>> Create(MovementCreateDto dto)
    {
        int userId = User.GetUserId();
        bool isAdmin = User.IsAdmin();

        if (dto.FromWarehouseId != null)
            await _warehouses.GetForUser(dto.FromWarehouseId.Value, userId, isAdmin);

        if (dto.ToWarehouseId != null)
            await _warehouses.GetForUser(dto.ToWarehouseId.Value, userId, isAdmin);

        StockMovement movement;

        if (dto.FromWarehouseId == null)
        {
            movement = await _stock.Receive(dto.ProductId, dto.ToWarehouseId!.Value, dto.Quantity, userId, dto.Comment);
        }
        else if (dto.ToWarehouseId == null)
        {
            movement = await _stock.Issue(dto.ProductId, dto.FromWarehouseId.Value, dto.Quantity, userId, dto.Comment);
        }
        else
        {
            movement = await _stock.Transfer(dto.ProductId, dto.FromWarehouseId.Value, dto.ToWarehouseId.Value, dto.Quantity, userId, dto.Comment);
        }

        var created = await _stock.Get(movement.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Mapping.ToDto(created));
    }
}
