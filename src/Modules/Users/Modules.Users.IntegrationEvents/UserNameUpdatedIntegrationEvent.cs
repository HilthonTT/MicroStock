using MicroStock.Common.Application.EventBus;

namespace Modules.Users.IntegrationEvents;

public sealed class UserNameUpdatedIntegrationEvent : IntegrationEvent
{
    public UserNameUpdatedIntegrationEvent(
        Guid id,
        DateTime occurredAtUtc,
        Guid userId,
        string firstName,
        string lastName) 
        : base(id, occurredAtUtc)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
    }

    public Guid UserId { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }
}
