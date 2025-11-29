using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroStock.Web.Infrastructure.Api;
using MicroStock.Web.Infrastructure.Authentication;
using MicroStock.Web.Infrastructure.Preferences;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;

namespace MicroStock.Web.Infrastructure;

public static class InfrastructureConfiguration
{
    private const string ClientName = "MicroStock.Client";

    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMudServices(configuration =>
        {
            configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            configuration.SnackbarConfiguration.HideTransitionDuration = 100;
            configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
            configuration.SnackbarConfiguration.VisibleStateDuration = 3000;
            configuration.SnackbarConfiguration.ShowCloseIcon = false;
        });
        services.AddBlazoredLocalStorage();
        services.AddAuthenticationInternal();

        services.AddHttpClient(ClientName, client =>
        {
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
            client.BaseAddress = new Uri(configuration["ApiBaseUrl"]!);
        })
        .AddHttpMessageHandler<JwtAuthenticationHeaderHandler>();

        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName));

        services.AddTransient<IApiClient, ApiClient>();
        services.AddTransient<IPreferenceManager, PreferenceManager>();

        return services;
    }
}
