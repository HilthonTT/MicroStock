using MicroStock.Common.Application.EventBus;
using MicroStock.Common.Application.Messaging;
using Modules.Users.Domain.DomainEvents;
using Modules.Users.IntegrationEvents;

namespace Modules.Users.Application.Authentication.RegisterUser;

internal sealed class UserCreatedDomainEventHandler(IEventBus eventBus) 
    : DomainEventHandler<UserCreatedDomainEvent>
{
    public override async Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new UserCreatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredAtUtc,
                domainEvent.UserId,
                domainEvent.Email,
                domainEvent.FirstName,
                domainEvent.LastName),
            cancellationToken);
    }
}
