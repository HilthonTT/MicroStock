using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.Cors;

internal sealed class CorsConfigureOptions(IConfiguration configuration) 
    : IConfigureNamedOptions<CorsOptions>
{
    private const string ConfigurationSectionName = "Cors";

    public void Configure(string? name, CorsOptions options)
    {
        Configure(options);
    }

    public void Configure(CorsOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
