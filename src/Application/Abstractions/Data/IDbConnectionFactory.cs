using System.Data.Common;

namespace Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
