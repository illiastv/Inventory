using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsController(ProductService products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? unit,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool desc = false)
    {
        var products = await _products.Search(search, unit, sortBy, desc);
        return Ok(Mapping.ToDtos(products));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _products.Get(id);
        return Ok(Mapping.ToDto(product));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
    {
        var product = await _products.Create(dto.Sku, dto.Name, dto.Unit);
        var result = Mapping.ToDto(product);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpdateDto dto)
    {
        var product = await _products.Update(id, dto.Name, dto.Unit);
        return Ok(Mapping.ToDto(product));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _products.Delete(id);
        return NoContent();
    }
}
