using Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

internal sealed class ConfigureJwtAuthOption(IConfiguration configuration) : IConfigureNamedOptions<JwtAuthOptions>
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
