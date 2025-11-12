using Microsoft.Extensions.DependencyInjection;
using MicroStock.Common.Application.EventBus;
using System.Collections.Concurrent;
using System.Reflection;

namespace MicroStock.Common.Infrastructure.Inbox;

public static class IntegrationEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IIntegrationEventHandler> GetHandlers(
        Type integrationEventType,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        Type[] integrationEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{integrationEventType.Name}",
            _ => assembly.GetTypes()
                .Where(handlerType => handlerType.IsAssignableTo(typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType)))
                .ToArray());

        return integrationEventHandlerTypes
            .Select(serviceProvider.GetRequiredService)
            .Select(integrationEventHandler => (integrationEventHandler as IIntegrationEventHandler)!)
            .ToList();
    }
}
