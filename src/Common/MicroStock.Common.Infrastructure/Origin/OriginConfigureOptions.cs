using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Origin;

namespace MicroStock.Common.Infrastructure.Origin;

internal sealed class OriginConfigureOptions(IConfiguration configuration)
     : IConfigureNamedOptions<OriginOptions>
{
    private const string ConfigurationSectionName = "Origin";

    public void Configure(string? name, OriginOptions options)
    {
        Configure(options);
    }

    public void Configure(OriginOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
