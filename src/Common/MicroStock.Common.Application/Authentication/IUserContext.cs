namespace MicroStock.Common.Application.Authentication;

public interface IUserContext
{
    Task<Guid> GetUserIdAsync(CancellationToken cancellationToken = default);
}
