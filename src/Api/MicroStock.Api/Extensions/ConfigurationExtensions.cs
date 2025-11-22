namespace MicroStock.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder, 
        IEnumerable<string> modules)
    {
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", false, true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", false, true);
        }
    }

    internal static string GetConnectionStringOrThrow(this IConfiguration configuration, string name)
    {
        string? connectionString = configuration.GetConnectionString(name);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        return connectionString;
    }
}
