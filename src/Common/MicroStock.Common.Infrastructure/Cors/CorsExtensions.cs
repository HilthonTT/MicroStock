using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.Cors;

internal static class CorsExtensions
{
    internal static IServiceCollection AddCorsInternal(this IServiceCollection services)
    {
        services.ConfigureOptions<CorsConfigureOptions>();

        using ServiceProvider sp = services.BuildServiceProvider();

        var corsOptions = sp.GetRequiredService<IOptions<CorsOptions>>().Value;

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins(corsOptions.AllowedOrigins);
            });
        });

        return services;
    }
}
