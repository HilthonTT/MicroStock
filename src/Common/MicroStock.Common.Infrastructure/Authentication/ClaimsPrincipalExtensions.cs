using MicroStock.Common.Application.Exceptions;
using System.Security.Claims;

namespace MicroStock.Common.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(CustomClaims.Sub);

        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : throw new MicroStockException("User identifier is unavailable");
    }

    public static string? GetIdentityId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static HashSet<string> GetPermissions(this ClaimsPrincipal? principal)
    {
        IEnumerable<Claim> permissionClaims = principal?.FindAll(CustomClaims.Permission) ??
                               throw new MicroStockException("Permissions are unavailable");

        return permissionClaims.Select(claim => claim.Value).ToHashSet();
    }
}
