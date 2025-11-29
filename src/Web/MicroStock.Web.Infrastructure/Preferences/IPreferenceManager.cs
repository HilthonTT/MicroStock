using MudBlazor;

namespace MicroStock.Web.Infrastructure.Preferences;

public interface IPreferenceManager
{
    Task SetPreferenceAsync(ClientPreference preference, CancellationToken cancellationToken = default);

    Task<ClientPreference> GetPreferenceAsync(CancellationToken cancellationToken = default);

    Task<bool> ChangeLanguageAsync(string languageCode, CancellationToken cancellationToken = default);

    Task<MudTheme> GetCurrentThemeAsync(CancellationToken cancellationToken = default);

    Task<bool> ToggleDarkModeAsync(CancellationToken cancellationToken = default);

    Task<bool> ToggleLayoutDirectionAsync(CancellationToken cancellationToken = default);

    Task<bool> ToggleDrawerAsync(CancellationToken cancellationToken = default);
}
