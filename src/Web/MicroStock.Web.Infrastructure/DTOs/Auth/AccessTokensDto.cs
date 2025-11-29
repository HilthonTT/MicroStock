namespace MicroStock.Web.Infrastructure.DTOs.Auth;

public sealed record AccessTokensDto
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }
}