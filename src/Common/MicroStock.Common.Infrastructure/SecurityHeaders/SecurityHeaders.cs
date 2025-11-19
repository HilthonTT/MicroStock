namespace MicroStock.Common.Infrastructure.SecurityHeaders;

public sealed class SecurityHeaders
{
    public string? XContentTypeOptions { get; init; }

    public string? ReferrerPolicy { get; init; }

    public string? XSSProtection { get; init; }

    public string? XFrameOptions { get; init; }

    public string? ContentSecurityPolicy { get; init; }

    public string? PermissionsPolicy { get; init; }

    public string? StrictTransportSecurity { get; init; }
}
