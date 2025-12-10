using Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider : ITokenProvider
{
    private readonly JwtAuthOptions _jwtAuthOptions;

    public TokenProvider(IOptions<JwtAuthOptions> options)
    {
        _jwtAuthOptions = options.Value;
    }

    public AccessTokensDto Create(TokenRequest tokenRequest)
    {
        return new AccessTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
    }

    private string GenerateAccessToken(TokenRequest tokenRequest)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, tokenRequest.IdentityId),
            new(CustomClaims.UserId, tokenRequest.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            ..tokenRequest.Roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];


        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.ExpirationInMinutes),
            SigningCredentials = signingCredentials,
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience
        };

        var jsonWebTokenHandler = new JsonWebTokenHandler();
        string accessToken = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);

        return accessToken;
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
