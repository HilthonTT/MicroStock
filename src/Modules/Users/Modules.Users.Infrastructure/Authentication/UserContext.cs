using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Caching;
using MicroStock.Common.Infrastructure.Authentication;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Authentication;

internal sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    UsersDbContext usersDbContext,
    ICacheService cacheService) : IUserContext
{
    private const string CacheKeyPrefix = "users:id";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        string? identityId = httpContextAccessor.HttpContext?.User.GetIdentityId();
        if (identityId is null)
        {
            return Guid.Empty;
        }

        string cacheKey = $"{CacheKeyPrefix}:{identityId}";
        Guid? userId = await cacheService.GetAsync<Guid>(cacheKey, cancellationToken);

        if (userId.HasValue && userId != Guid.Empty)
        {
            return userId.Value;
        }

        userId = await usersDbContext.Users
            .Where(u => u.IdentityId == identityId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (userId.HasValue)
        {
            await cacheService.SetAsync(cacheKey, userId, CacheDuration, cancellationToken);
        }

        return userId ?? Guid.Empty;
    }
}
