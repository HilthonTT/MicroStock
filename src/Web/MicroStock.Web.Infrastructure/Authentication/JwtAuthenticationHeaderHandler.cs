using Microsoft.AspNetCore.Components;
using MicroStock.Web.Infrastructure.DTOs.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace MicroStock.Web.Infrastructure.Authentication;

internal sealed class JwtAuthenticationHeaderHandler : DelegatingHandler
{
    private const string Scheme = "Bearer";

    private readonly IAuthenticationService _authenticationService;
    private readonly NavigationManager _navigationManager;

    public JwtAuthenticationHeaderHandler(
        IAuthenticationService authenticationService,
        NavigationManager navigationManager)
    {
        _authenticationService = authenticationService;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (IsAuthenticationEndpoint(request.RequestUri))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        AccessTokensDto tokens = await _authenticationService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(tokens.AccessToken))
        {
            _navigationManager.NavigateTo("/login");
            return CreateEmptyResponse(request);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, tokens.AccessToken);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _authenticationService.LogoutAsync();
            _navigationManager.NavigateTo("/login");
            return CreateEmptyResponse(request);
        }

        return response;
    }

    private static bool IsAuthenticationEndpoint(Uri? uri)
    {
        if (uri is null)
        {
            return false;
        }

        string path = uri.AbsolutePath;
        return path.Contains("/auth", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/register", StringComparison.OrdinalIgnoreCase);
    }

    private static HttpResponseMessage CreateEmptyResponse(HttpRequestMessage request)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            RequestMessage = request
        };
        return response;
    }
}
