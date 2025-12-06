namespace Application.Abstractions.Authentication;

public sealed record AccessTokensDto(string AccessToken, string RefreshToken);
