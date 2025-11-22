using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.Mail;

internal sealed class MailConfigureOptions(IConfiguration configuration) : IConfigureNamedOptions<MailOptions>
{
    private const string ConfigurationSectionName = "Mail";

    public void Configure(string? name, MailOptions options)
    {
        Configure(options);
    }

    public void Configure(MailOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
