using InventoryLab3.Dtos;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Controllers;

[ApiController]
[Route("api/auth")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly UserService _users;

    public AuthController(AuthService auth, UserService users)
    {
        _auth = auth;
        _users = users;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        var user = await _auth.Register(dto.Email, dto.Password);
        var result = Mapping.ToDto(user);
        return Created($"/api/users/{result.Id}", result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var result = await _auth.Login(dto.Email, dto.Password);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await _users.Get(User.GetUserId());
        return Ok(Mapping.ToDto(user));
    }
}
