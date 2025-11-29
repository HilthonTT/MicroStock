using MicroStock.Web.Infrastructure.DTOs.Auth;
using System.Net.Http.Json;
using System.Text.Json;

namespace MicroStock.Web.Infrastructure.Api;

internal sealed class ApiClient : IApiClient
{
    private readonly static JsonSerializerOptions _jsonSerializerOptions = new();
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AccessTokensDto> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        using var response = await _httpClient.PostAsJsonAsync("auth/login", request, _jsonSerializerOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AccessTokensDto>(_jsonSerializerOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize access tokens.");
    }

    public async Task<AccessTokensDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        using var response = await _httpClient.PostAsJsonAsync("auth/register", request, _jsonSerializerOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AccessTokensDto>(_jsonSerializerOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize access tokens.");
    }

    public async Task<AccessTokensDto> RefreshTokensAsync(RefreshUserTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        using var response = await _httpClient.PostAsJsonAsync("auth/refresh", request, _jsonSerializerOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AccessTokensDto>(_jsonSerializerOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize access tokens.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Request failed with status {(int)response.StatusCode} ({response.StatusCode}). Response: {errorContent}");
    }
}
