using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Caching;

internal static class CacheOptions
{
    private static readonly DistributedCacheEntryOptions DefaultExpiration = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
    };

    public static DistributedCacheEntryOptions Create(TimeSpan? absoluteExpirationRelativeToNow)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
                ?? DefaultExpiration.AbsoluteExpirationRelativeToNow,
        };
    }
}