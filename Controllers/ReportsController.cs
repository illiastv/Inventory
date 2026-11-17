using System.ComponentModel.DataAnnotations;
using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = Policies.AdminOnly)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reports;

    public ReportsController(ReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("turnover")]
    public async Task<ActionResult<List<WarehouseTurnover>>> GetTurnover()
    {
        var result = await _reports.GetTurnoverPerWarehouse();
        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<List<TopProductRow>>> GetTopProducts([FromQuery, Range(1, 100)] int take = 5)
    {
        var result = await _reports.GetTopProducts(take);
        return Ok(result);
    }
}
