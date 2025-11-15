using MicroStock.Common.Domain;

namespace Modules.Users.Domain.DomainEvents;

public sealed class UserCreatedDomainEvent(Guid userId, string email) : DomainEvent
{
    public Guid UserId { get; init; } = userId;

    public string Email { get; init; } = email;
}
