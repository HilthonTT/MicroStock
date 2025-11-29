using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MicroStock.Web.Infrastructure.Api;
using MicroStock.Web.Infrastructure.DTOs.Auth;
using MicroStock.Web.Infrastructure.Storage;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MicroStock.Web.Infrastructure.Authentication;

internal sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable, IAuthenticationService
{
    private const string AuthenticationType = "jwt";

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IApiClient _apiClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    private bool _isRefreshing = false;

    public JwtAuthenticationStateProvider(
        IApiClient apiClient,
        ILocalStorageService localStorage,
        NavigationManager navigationManager)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            string? accessToken = await _localStorage.GetItemAsStringAsync(StorageConstants.Local.AccessToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Anonymous();
            }

            JsonWebToken jwtToken;

            try
            {
                jwtToken = new JsonWebToken(accessToken);
            }
            catch
            {
                await ClearAuthDataAsync();
                return Anonymous();
            }

            // Check if token is expired or will expire in the next 30 seconds
            if (jwtToken.ValidTo < DateTime.UtcNow.AddSeconds(30))
            {
                bool refreshed = await TryRefreshTokenAsync();

                if (!refreshed)
                {
                    await ClearAuthDataAsync();
                    return Anonymous();
                }

                accessToken = await _localStorage.GetItemAsStringAsync(StorageConstants.Local.AccessToken);
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return Anonymous();
                }

                jwtToken = new JsonWebToken(accessToken);
            }

            var identity = new ClaimsIdentity(jwtToken.Claims, AuthenticationType);

            var roles = await GetCachedRolesAsync();
            if (roles.Count > 0)
            {
                identity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // Any unexpected error → force logout
            await ClearAuthDataAsync();
            return Anonymous();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        // Prevent multiple simultaneous refresh attempts
        if (_isRefreshing)
        {
            return false;
        }

        _isRefreshing = true;

        try
        {
            string? refreshToken = await _localStorage.GetItemAsStringAsync(StorageConstants.Local.RefreshToken);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            AccessTokensDto response = await _apiClient.RefreshTokensAsync(new RefreshUserTokenRequest
            {
                RefreshToken = refreshToken
            });

            if (response?.AccessToken == null || response?.RefreshToken == null)
            {
                return false;
            }

            await _localStorage.SetItemAsStringAsync(StorageConstants.Local.AccessToken, response.AccessToken);
            await _localStorage.SetItemAsStringAsync(StorageConstants.Local.RefreshToken, response.RefreshToken);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public async Task LoginAsync(AccessTokensDto tokens)
    {
        await _localStorage.SetItemAsStringAsync(StorageConstants.Local.AccessToken, tokens.AccessToken);
        await _localStorage.SetItemAsStringAsync(StorageConstants.Local.RefreshToken, tokens.RefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await ClearAuthDataAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));

        _navigationManager.NavigateTo("/login");
    }

    public async Task<AccessTokensDto> GetTokenAsync()
    {
        string? accessToken = await _localStorage.GetItemAsStringAsync(StorageConstants.Local.AccessToken);
        string? refreshToken = await _localStorage.GetItemAsStringAsync(StorageConstants.Local.RefreshToken);

        return new AccessTokensDto
        {
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty,
        };
    }

    private async Task ClearAuthDataAsync()
    {
        await _localStorage.RemoveItemsAsync(
        [
            StorageConstants.Local.AccessToken,
            StorageConstants.Local.RefreshToken,
            StorageConstants.Local.Roles
        ]);
    }

    private static AuthenticationState Anonymous() => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private async Task<List<string>> GetCachedRolesAsync()
    {
        return await _localStorage.GetItemAsync<List<string>>(StorageConstants.Local.Roles) ?? [];
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
