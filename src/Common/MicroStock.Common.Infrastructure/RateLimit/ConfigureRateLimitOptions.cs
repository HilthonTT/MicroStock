using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.RateLimit;

internal sealed class ConfigureRateLimitOptions(IConfiguration configuration) : IConfigureNamedOptions<RateLimitOptions>
{
    private const string ConfigurationSectionName = "RateLimiting";

    public void Configure(string? name, RateLimitOptions options)
    {
        Configure(options);
    }

    public void Configure(RateLimitOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
