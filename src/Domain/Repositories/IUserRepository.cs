using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);

    Task InsertAsync(User user, CancellationToken cancellationToken = default);
}
