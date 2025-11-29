using Blazored.LocalStorage;
using MudBlazor;

namespace MicroStock.Web.Infrastructure.Preferences;

internal sealed class PreferenceManager : IPreferenceManager
{
    private readonly ILocalStorageService _localStorageService;

    public PreferenceManager(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public Task<bool> ChangeLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MudTheme> GetCurrentThemeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientPreference> GetPreferenceAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SetPreferenceAsync(ClientPreference preference, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ToggleDarkModeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ToggleDrawerAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ToggleLayoutDirectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
