using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authentication.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    UserManager<IdentityUser> userManager,
    IDbContext dbContext) : ICommandHandler<RegisterUserCommand>
{
    public async Task<Result> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        IdentityUser? existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            return Result.Failure(AuthErrors.EmailAlreadyInUse);
        }

        var identityUser = new IdentityUser
        {
            Email = command.Email,
            UserName = command.Email
        };

        IdentityResult createUserResult = await userManager.CreateAsync(identityUser, command.Password);
        if (!createUserResult.Succeeded)
        {
            if (createUserResult.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return Result.Failure(AuthErrors.EmailAlreadyInUse);
            }

            return Result.Failure(AuthErrors.UnableToRegister);
        }

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            return Result.Failure(AuthErrors.UnableToRegister);
        }

        User user = User.Create(identityUser.Id, command.Name, command.Email);

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
