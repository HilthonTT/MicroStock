using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class JwtAuditingUserProvider(IHttpContextAccessor httpContextAccessor) : IAuditingUserProvider
{
    private const string DefaultUser = "Unknown User";

    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.GetUserId().ToString() ?? DefaultUser;
    }
}
