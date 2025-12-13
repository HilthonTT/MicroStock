using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.Authentication.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IIdentityDbContext identityDbContext,
    IDbContext dbContext,
    ITokenProvider tokenProvider,
    UserManager<IdentityUser> userManager,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtAuthOptions> options) : ICommandHandler<RefreshTokenCommand, AccessTokensDto>
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokensDto>> Handle(
        RefreshTokenCommand command, 
        CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await identityDbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < dateTimeProvider.UtcNow)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToLogin);
        }

        User? appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == refreshToken.UserId, cancellationToken);
        if (appUser is null)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToLogin);
        }

        IList<string> roles = await userManager.GetRolesAsync(refreshToken.User);

        var tokenRequest = new TokenRequest(appUser.Id, refreshToken.User.Id, refreshToken.User.Email!, roles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        refreshToken.Refresh(
            accessTokens.RefreshToken,
            DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays));

        await identityDbContext.SaveChangesAsync(cancellationToken);

        return accessTokens;
    }
}
