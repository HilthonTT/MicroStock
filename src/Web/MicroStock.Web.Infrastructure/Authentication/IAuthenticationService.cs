using MicroStock.Web.Infrastructure.DTOs.Auth;

namespace MicroStock.Web.Infrastructure.Authentication;

public interface IAuthenticationService
{
    Task LoginAsync(AccessTokensDto tokens);

    Task<AccessTokensDto> GetTokenAsync();

    Task LogoutAsync();
}
