using MicroStock.Common.Application.EventBus;

namespace Modules.Users.IntegrationEvents;

public sealed class UserEmailUpdatedDomainEvent : IntegrationEvent
{
    public UserEmailUpdatedDomainEvent(
       Guid id,
       DateTime occurredAtUtc,
       Guid userId,
       string newEmail)
       : base(id, occurredAtUtc)
    {
        UserId = userId;
        NewEmail = newEmail;
    }

    public Guid UserId { get; init; }

    public string NewEmail { get; init; }
}
