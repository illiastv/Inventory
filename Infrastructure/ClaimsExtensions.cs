using System.Security.Claims;
using InventoryLab3.Domain;

namespace InventoryLab3.Infrastructure;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(value, out int id))
            throw new UnauthorizedException("Токен не містить ідентифікатора користувача");

        return id;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin);
    }
}
