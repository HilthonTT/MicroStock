namespace MicroStock.Web.Infrastructure.DTOs.Auth;

public sealed record RefreshUserTokenRequest
{
    public required string RefreshToken { get; set; }
}
