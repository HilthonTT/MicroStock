using MicroStock.Web.Infrastructure.DTOs.Auth;

namespace MicroStock.Web.Infrastructure.Api;

public interface IApiClient
{
    Task<AccessTokensDto> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default);

    Task<AccessTokensDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<AccessTokensDto> RefreshTokensAsync(RefreshUserTokenRequest request, CancellationToken cancellationToken = default);
}
