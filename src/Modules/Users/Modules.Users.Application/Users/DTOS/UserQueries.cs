using Modules.Users.Application.Users.GetProfile;
using Modules.Users.Domain.Entities;
using System.Linq.Expressions;

namespace Modules.Users.Application.Users.DTOS;

internal static class UserQueries
{
    public static Expression<Func<User, UserDto>> ProjectToDto()
    {
        return u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            CreatedAtUtc = u.CreatedAtUtc,
        };
    }
}
