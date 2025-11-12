using MassTransit;
using MicroStock.Common.Application.EventBus;

namespace MicroStock.Common.Infrastructure.EventBus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
    {
        return bus.Publish(integrationEvent, cancellationToken);
    }
}
