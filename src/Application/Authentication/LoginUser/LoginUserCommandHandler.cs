using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.Authentication.LoginUser;

internal sealed class LoginUserCommandHandler(
    UserManager<IdentityUser> userManager,
    ITokenProvider tokenProvider,
    IIdentityDbContext identityDbContext,
    IDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtAuthOptions> options) : ICommandHandler<LoginUserCommand, AccessTokensDto>
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokensDto>> Handle(
        LoginUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(command.Email);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, command.Password))
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToRegister);
        }

        User? appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (appUser is null)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToLogin);
        }

        if (!appUser.EmailVerified)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.EmailMustBeVerified);
        }

        IList<string> roles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(appUser.Id, identityUser.Id, identityUser.Email ?? string.Empty, roles);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = RefreshToken.Create(
            identityUser.Id,
            accessTokens.RefreshToken,
            dateTimeProvider.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays));

        await identityDbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        return accessTokens;
    }
}
