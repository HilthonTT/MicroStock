namespace Infrastructure.Cors;

internal sealed class CorsOptions
{
    public const string ConfigurationSectionName = "Cors";
    public const string PolicyName = "CorsPolicy";

    public string[] AllowedOrigins { get; init; } = [];
}
