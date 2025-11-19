namespace MicroStock.Common.Infrastructure.SecurityHeaders;

public sealed class SecurityHeaderOptions
{
    public required bool Enabled { get; set; }

    public required SecurityHeaders Headers { get; set; }
}
