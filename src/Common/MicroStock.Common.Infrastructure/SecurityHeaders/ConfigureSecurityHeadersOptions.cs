using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.SecurityHeaders;

internal sealed class ConfigureSecurityHeadersOptions(IConfiguration configuration) : IConfigureNamedOptions<SecurityHeaderOptions>
{
    private const string ConfigurationSectionName = "SecurityHeaders";

    public void Configure(string? name, SecurityHeaderOptions options)
    {
        Configure(options);
    }

    public void Configure(SecurityHeaderOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
