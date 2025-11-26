using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    UserManager<IdentityUser> userManager,
    IDbContext dbContext,
    IIdentityDbContext identityDbContext) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await dbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

        IdentityUser? identityUser = await userManager.FindByEmailAsync(command.Email);

        if (identityUser is null)
        {
            return Result.Failure(UserErrors.NotFoundEmail(command.Email));
        }

        IdentityResult result = await userManager.ChangePasswordAsync(identityUser, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(AuthErrors.Unauthorized);
        }

        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFoundEmail(command.Email));
        }

        user.UpdateEmail(command.Email);

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
