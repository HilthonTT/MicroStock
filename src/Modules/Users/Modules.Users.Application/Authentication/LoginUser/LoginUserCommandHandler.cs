using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Authentication.LoginUser;

internal sealed class LoginUserCommandHandler(
    UserManager<IdentityUser> userManager, 
    ITokenProvider tokenProvider,
    IIdentityDbContext identityDbContext,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtAuthOptions> options) 
    : ICommandHandler<LoginUserCommand, AccessTokensDto>
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokensDto>> Handle(
        LoginUserCommand command, 
        CancellationToken cancellationToken)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(command.Email);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, command.Password))
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.Unauthorized);
        }

        IList<string> roles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!, roles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            userId: identityUser.Id,
            token: accessTokens.RefreshToken,
            expiresAtUtc: dateTimeProvider.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays),
            user: identityUser);

        await identityDbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        await identityDbContext.SaveChangesAsync(cancellationToken);

        return accessTokens;
    }
}
