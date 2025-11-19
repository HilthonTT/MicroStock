using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MicroStock.Common.Infrastructure.Authentication;
using System.Threading.RateLimiting;

namespace MicroStock.Common.Infrastructure.RateLimit;

public static class RateLimitExtensions
{
    internal static IServiceCollection ConfigureRateLimit(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureOptions<ConfigureRateLimitOptions>();

        using ServiceProvider sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        if (!options.Enabled)
        {
            return services;
        }

        services.AddRateLimiter(rateLimitOptions =>
        {
            rateLimitOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowInSeconds)
                    });
            });

            rateLimitOptions.RejectionStatusCode = options.RejectionStatusCode;
            rateLimitOptions.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
                    ProblemDetailsFactory problemDetailsFactory = 
                        context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests, "Too Many Requests",
                        detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.");

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
                }
            };

            rateLimitOptions.AddPolicy("default", context =>
            {
                string userName = context.User.GetIdentityId() ?? string.Empty;
                if (!string.IsNullOrEmpty(userName))
                    return RateLimitPartition.GetTokenBucketLimiter(userName, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 100,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 25
                        });
                return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });

        return services;
    }
}
