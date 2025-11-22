using Microsoft.Extensions.Logging;
using MicroStock.Common.Application.Messaging;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Application.Authentication.RegisterUser;

internal sealed class UserCreatedDomainEventHandler(ILogger<UserCreatedDomainEventHandler> logger) 
    : DomainEventHandler<UserCreatedDomainEvent>
{
    public override Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("User created with email: '{Email}'", domainEvent.Email);
        return Task.CompletedTask;
    }
}
