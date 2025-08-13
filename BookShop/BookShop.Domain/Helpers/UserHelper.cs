using System.Security.Claims;

namespace BookShop.Domain.Helpers;

public static class UserHelper
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(id, out var guid)
            ? guid
            : throw new UnauthorizedAccessException("Invalid user id.");
    }
}