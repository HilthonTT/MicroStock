using Microsoft.EntityFrameworkCore;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Domain;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Users.DTOS;
using Modules.Users.Application.Users.GetProfile;
using Modules.Users.Domain.Errors;

namespace Modules.Users.Application.Users.GetUser;

internal sealed class GetUserQueryHandler(IDbContext dbContext) : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        UserDto? user = await dbContext.Users
            .Where(u => u.Id == query.UserId)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
