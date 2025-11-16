using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;

namespace Modules.Users.Application.Authentication.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IIdentityDbContext identityDbContext,
    UserManager<IdentityUser> userManager,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtAuthOptions> options) : ICommandHandler<RefreshTokenCommand, AccessTokensDto>
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokensDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.RefreshToken? refreshToken = await identityDbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < dateTimeProvider.UtcNow)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.Unauthorized);
        }

        IList<string> roles = await userManager.GetRolesAsync(refreshToken.User);

        var tokenRequest = new TokenRequest(refreshToken.User.Id, refreshToken.User.Email!, roles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        refreshToken.Refresh(accessTokens.RefreshToken, dateTimeProvider.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays));

        await identityDbContext.SaveChangesAsync(cancellationToken);

        return accessTokens;
    }
}
