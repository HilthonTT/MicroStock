using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Abstractions.Data;

public interface IDbContext
{
    DbSet<User> Users { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
