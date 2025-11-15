namespace MicroStock.Common.Infrastructure.Cors;

public sealed class CorsOptions
{
    public required string[] AllowedOrigins { get; init; }
}
