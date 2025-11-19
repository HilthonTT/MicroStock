using Microsoft.AspNetCore.Http;

namespace MicroStock.Common.Infrastructure.RateLimit;

public sealed class RateLimitOptions
{
    public required bool Enabled { get; init; }

    public required int PermitLimit { get; init; }

    public required int WindowInSeconds { get; init; }

    public required int RejectionStatusCode { get; init; } = StatusCodes.Status429TooManyRequests;
}
