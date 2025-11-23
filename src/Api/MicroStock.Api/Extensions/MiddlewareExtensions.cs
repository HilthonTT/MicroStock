using MicroStock.Api.Middleware;

namespace MicroStock.Api.Extensions;

internal static class MiddlewareExtensions
{
    internal static IApplicationBuilder UseETag(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ETagMiddleware>();
    }

    internal static IApplicationBuilder UseUserContextEnrichment(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextEnrichmentMiddleware>();
    }

    internal static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }
}
