using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public class WarehousesController : ControllerBase
{
    private readonly WarehouseService _warehouses;
    private readonly StockService _stock;

    public WarehousesController(WarehouseService warehouses, StockService stock)
    {
        _warehouses = warehouses;
        _stock = stock;
    }

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? city,
        [FromQuery] int? ownerId,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool desc = false)
    {
        int? owner = ownerId;
        if (!User.IsAdmin())
            owner = User.GetUserId();

        var warehouses = await _warehouses.Search(search, city, owner, sortBy, desc);
        return Ok(Mapping.ToDtos(warehouses));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id)
    {
        var warehouse = await _warehouses.GetForUser(id, User.GetUserId(), User.IsAdmin());
        return Ok(Mapping.ToDto(warehouse));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<WarehouseDto>> Create(WarehouseCreateDto dto)
    {
        var warehouse = await _warehouses.Create(dto.Name, dto.City, User.GetUserId());
        var result = Mapping.ToDto(warehouse);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, WarehouseUpdateDto dto)
    {
        await _warehouses.GetForUser(id, User.GetUserId(), User.IsAdmin());
        var warehouse = await _warehouses.Update(id, dto.Name, dto.City);
        return Ok(Mapping.ToDto(warehouse));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _warehouses.GetForUser(id, User.GetUserId(), User.IsAdmin());
        await _warehouses.Delete(id);
        return NoContent();
    }

    [HttpGet("{id:int}/stock")]
    public async Task<ActionResult<List<StockBalance>>> GetStock(int id)
    {
        await _warehouses.GetForUser(id, User.GetUserId(), User.IsAdmin());
        var balances = await _stock.GetBalances(id);
        return Ok(balances);
    }
}
