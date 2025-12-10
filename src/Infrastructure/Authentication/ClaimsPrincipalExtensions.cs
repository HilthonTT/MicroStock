using System.Security.Claims;

namespace Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string? GetIdentityId(this ClaimsPrincipal? claimsPrincipal)
    {
        string? identityId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return identityId;
    }

    public static Guid GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        string? userId = claimsPrincipal?.FindFirstValue(CustomClaims.UserId);
        return Guid.TryParse(userId, out Guid parsedUserId)
             ? parsedUserId
             : Guid.Empty;
    }
}
