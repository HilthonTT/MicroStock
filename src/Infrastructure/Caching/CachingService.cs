using Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Buffers;
using System.Text.Json;

namespace Infrastructure.Caching;

internal sealed class CachingService(IDistributedCache cache) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        byte[]? bytes = await cache.GetAsync(key, cancellationToken);

        return bytes is null ? default : Deserialize<T>(bytes);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return cache.RemoveAsync(key, cancellationToken);
    }

    public Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        byte[] bytes = Serialize(value);

        return cache.SetAsync(key, bytes, CacheOptions.Create(absoluteExpirationRelativeToNow), cancellationToken);
    }

    private static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value, SerializerOptions);
        return buffer.WrittenSpan.ToArray();
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        var reader = new Utf8JsonReader(bytes);
        return JsonSerializer.Deserialize<T>(ref reader, SerializerOptions)
             ?? throw new InvalidOperationException($"Failed to deserialize cached value for type {typeof(T).Name}");
    }
}
