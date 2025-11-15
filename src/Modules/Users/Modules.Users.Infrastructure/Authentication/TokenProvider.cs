using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MicroStock.Common.Domain;
using MicroStock.Common.Infrastructure.Authentication;
using Modules.Users.Application.Abstractions.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Modules.Users.Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<JwtAuthOptions> options, IDateTimeProvider dateTimeProvider) : ITokenProvider
{
    private readonly JwtAuthOptions _options = options.Value;

    public AccessTokensDto Create(TokenRequest tokenRequest)
    {
        return new AccessTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
    }

    private string GenerateAccessToken(TokenRequest request)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(JwtRegisteredClaimNames.Email, request.Email),
            ..request.Roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = dateTimeProvider.UtcNow.AddMinutes(_options.ExpirationInMinutes),
            SigningCredentials = signingCredentials,
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var tokenHandler = new JsonWebTokenHandler();
        string accessToken = tokenHandler.CreateToken(securityTokenDescriptor);

        return accessToken;
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(randomBytes);
    }
}
