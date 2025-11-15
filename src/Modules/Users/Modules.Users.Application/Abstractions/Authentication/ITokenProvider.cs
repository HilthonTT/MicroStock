namespace Modules.Users.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    AccessTokensDto Create(TokenRequest request);
}
