using MicroStock.Common.Application.EventBus;

namespace Modules.Users.IntegrationEvents;

public sealed class UserCreatedIntegrationEvent : IntegrationEvent
{
    public UserCreatedIntegrationEvent(
        Guid id, 
        DateTime occurredAtUtc, 
        Guid userId, 
        string email, 
        string firstName, 
        string lastName)
        : base(id, occurredAtUtc)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public Guid UserId { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }
}
