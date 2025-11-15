using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.Authentication;

internal sealed class JwtAuthConfigureOptions(IConfiguration configuration)
    : IConfigureNamedOptions<JwtAuthOptions>
{
    private const string ConfigurationSectionName = "Jwt";

    public void Configure(string? name, JwtAuthOptions options)
    {
        Configure(options);
    }

    public void Configure(JwtAuthOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
