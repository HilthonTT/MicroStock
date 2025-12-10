using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public string IdentityId =>
        httpContextAccessor.HttpContext?.User.GetIdentityId() ?? string.Empty;

    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
}
