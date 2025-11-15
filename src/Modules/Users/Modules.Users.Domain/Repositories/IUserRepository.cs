using Modules.Users.Domain.Entities;

namespace Modules.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(User user);
}
