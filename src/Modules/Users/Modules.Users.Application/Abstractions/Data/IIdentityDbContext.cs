using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Application.Abstractions.Data;

public interface IIdentityDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
