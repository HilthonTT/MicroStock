using SharedKernel;

namespace Domain.DomainEvents;

public sealed class UserCreatedDomainEvent(Guid userId, string identityId, string email) : DomainEvent
{
    public Guid UserId { get; init; } = userId;

    public string IdentityId { get; init; } = identityId;

    public string Email { get; init; } = email;
}
