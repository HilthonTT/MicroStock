namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    AccessTokensDto Create(TokenRequest tokenRequest);
}
