using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MicroStock.Web.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
        services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();

        return services;
    }
}
