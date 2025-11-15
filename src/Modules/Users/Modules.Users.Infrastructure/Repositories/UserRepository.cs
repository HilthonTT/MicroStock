using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    public Task<User?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public void Insert(User user)
    {
        dbContext.Users.Add(user);
    }
}
