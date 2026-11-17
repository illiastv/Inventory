using InventoryLab3.Domain;
using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.AdminOnly)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class UsersController : ControllerBase
{
    private readonly UserService _users;

    public UsersController(UserService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _users.GetAll();
        return Ok(Mapping.ToDtos(users));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _users.Get(id);
        return Ok(Mapping.ToDto(user));
    }

    [HttpPut("{id:int}/role")]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> SetRole(int id, RoleUpdateDto dto)
    {
        if (id == User.GetUserId())
            throw new ConflictException("Не можна змінити власну роль");

        var user = await _users.SetRole(id, dto.Role);
        return Ok(Mapping.ToDto(user));
    }
}
