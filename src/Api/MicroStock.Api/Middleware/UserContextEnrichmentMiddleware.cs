using MicroStock.Common.Application.Authentication;
using System.Diagnostics;

namespace MicroStock.Api.Middleware;

internal sealed class UserContextEnrichmentMiddleware(
    RequestDelegate next,
    ILogger<UserContextEnrichmentMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        Guid userId = await userContext.GetUserIdAsync();

        if (userId == Guid.Empty)
        {
            Activity.Current?.SetTag("user.id", userId);

            var state = new Dictionary<string, object>
            {
                ["UserId"] = userId,
            };
            using (logger.BeginScope(state))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
