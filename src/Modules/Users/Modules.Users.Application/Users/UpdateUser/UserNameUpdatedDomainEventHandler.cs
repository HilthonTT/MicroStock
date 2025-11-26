using MicroStock.Common.Application.EventBus;
using MicroStock.Common.Application.Messaging;
using Modules.Users.Domain.DomainEvents;
using Modules.Users.IntegrationEvents;

namespace Modules.Users.Application.Users.UpdateUser;

internal sealed class UserNameUpdatedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<UserNameUpdatedDomainEvent>
{
    public override async Task Handle(UserNameUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(new UserNameUpdatedIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredAtUtc,
            domainEvent.UserId,
            domainEvent.FirstName,
            domainEvent.LastName),
            cancellationToken);
    }
}
