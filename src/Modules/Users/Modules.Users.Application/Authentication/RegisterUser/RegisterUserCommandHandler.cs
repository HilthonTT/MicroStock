using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Authentication.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    UserManager<IdentityUser> userManager,
    IDbContext dbContext,
    IIdentityDbContext identityDbContext,
    IDateTimeProvider dateTimeProvider,
    ITokenProvider tokenProvider,
    IOptions<JwtAuthOptions> options) : ICommandHandler<RegisterUserCommand, AccessTokensDto>
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public async Task<Result<AccessTokensDto>> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await dbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

        var identityUser = new IdentityUser
        {
            Email = command.Email,
            UserName = command.Email,
        };

        IdentityResult createUserResult = await userManager.CreateAsync(identityUser, command.Password);

        if (!createUserResult.Succeeded)
        {
            bool emailAlreadyExists = createUserResult.Errors.Any(e =>
                e.Code == nameof(IdentityErrorDescriber.DuplicateEmail) ||
                e.Code == "DuplicateEmail");

            bool usernameAlreadyExists = createUserResult.Errors.Any(e =>
                e.Code == nameof(IdentityErrorDescriber.DuplicateUserName) ||
                e.Code == "DuplicateUserName");

            if (emailAlreadyExists || usernameAlreadyExists)
            {
                return Result.Failure<AccessTokensDto>(AuthErrors.AlreadyRegistered);
            }

            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToRegister);
        }

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);

        if (!addToRoleResult.Succeeded)
        {
            return Result.Failure<AccessTokensDto>(AuthErrors.UnableToRegister);
        }

        User user = User.Create(
            identityId: identityUser.Id,
            email: command.Email,
            firstName: command.FirstName,
            lastName: command.LastName,
            createdAtUtc: dateTimeProvider.UtcNow);

        await dbContext.Users.AddAsync(user, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email, [Roles.Member]);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            userId: identityUser.Id,
            token: accessTokens.RefreshToken,
            expiresAtUtc: dateTimeProvider.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays),
            user: identityUser);

        await identityDbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        await identityDbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return accessTokens;
    }
}
