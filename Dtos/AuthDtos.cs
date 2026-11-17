using System.ComponentModel.DataAnnotations;

namespace InventoryLab3.Dtos;

public class RegisterDto
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Пароль має містити літери і цифри")]
    public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(100)]
    public string Password { get; set; } = "";
}

public class RoleUpdateDto
{
    [Required, RegularExpression("^(User|Admin)$", ErrorMessage = "Роль може бути: User або Admin")]
    public string Role { get; set; } = "";
}

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);

public record UserDto(int Id, string Email, string Role);
