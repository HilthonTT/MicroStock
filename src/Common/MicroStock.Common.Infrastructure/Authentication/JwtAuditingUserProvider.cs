using Microsoft.AspNetCore.Http;
using MicroStock.Common.Application.Exceptions;
using MicroStock.Common.Infrastructure.Auditing;

namespace MicroStock.Common.Infrastructure.Authentication;

internal sealed class JwtAuditingUserProvider(IHttpContextAccessor httpContextAccessor) : IAuditingUserProvider
{
    private const string DefaultUser = "Unknown User";

    public string GetUserId()
    {
        try
        {
            return httpContextAccessor.HttpContext?.User.GetUserId().ToString() ?? DefaultUser;
        }
        catch (MicroStockException)
        {
            return DefaultUser;
        }
    }
}
