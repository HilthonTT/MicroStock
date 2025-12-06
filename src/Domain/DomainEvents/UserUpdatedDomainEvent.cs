using SharedKernel;

namespace Domain.DomainEvents;

public sealed class UserUpdatedDomainEvent(Guid userId, string username) : DomainEvent
{
    public Guid UserId { get; init; } = userId;

    public string Username { get; init; } = username;
}
