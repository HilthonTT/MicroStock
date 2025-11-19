using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroStock.Common.Infrastructure.SecurityHeaders;

public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureSecurityHeadersOptions>();

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        SecurityHeaderOptions options = 
            app.ApplicationServices.GetRequiredService<IOptions<SecurityHeaderOptions>>().Value;

        if (!options.Enabled)
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            if (!context.Response.HasStarted)
            {
                if (!string.IsNullOrWhiteSpace(options.Headers.XFrameOptions))
                {
                    context.Response.Headers.XFrameOptions = options.Headers.XFrameOptions;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.XContentTypeOptions))
                {
                    context.Response.Headers.XContentTypeOptions = options.Headers.XContentTypeOptions;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.ReferrerPolicy))
                {
                    context.Response.Headers.Referer = options.Headers.ReferrerPolicy;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.PermissionsPolicy))
                {
                    context.Response.Headers["Permissions-Policy"] = options.Headers.PermissionsPolicy;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.XSSProtection))
                {
                    context.Response.Headers.XXSSProtection = options.Headers.XSSProtection;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.ContentSecurityPolicy))
                {
                    context.Response.Headers.ContentSecurityPolicy = options.Headers.ContentSecurityPolicy;
                }

                if (!string.IsNullOrWhiteSpace(options.Headers.StrictTransportSecurity))
                {
                    context.Response.Headers.StrictTransportSecurity = options.Headers.StrictTransportSecurity;
                }
            }

            await next.Invoke();
        });

        return app;
    }
}
