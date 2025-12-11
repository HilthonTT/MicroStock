using Application.Abstractions.EventBus;

namespace Infrastructure.Inbox;

public interface IIntegrationEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default);

    Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
