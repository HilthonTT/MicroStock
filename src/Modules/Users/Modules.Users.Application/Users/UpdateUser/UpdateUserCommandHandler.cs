using Microsoft.EntityFrameworkCore;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Users.GetProfile;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;

namespace Modules.Users.Application.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(IDbContext dbContext) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
           .Where(u => u.Id == command.UserId)
           .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(command.UserId));
        }

        user.UpdateName(command.FirstName, command.LastName);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
