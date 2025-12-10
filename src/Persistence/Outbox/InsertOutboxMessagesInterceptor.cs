using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Persistence.Serialization;
using SharedKernel;

namespace Persistence.Outbox;

internal sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await InsertOutboxMessagesAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task InsertOutboxMessagesAsync(DbContext context, CancellationToken cancellationToken)
    {
        List<OutboxMessage> outboxMessages = context
            .ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IDomainEvent> domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                Type = domainEvent.GetType().FullName!,
                Content = JsonConvert.SerializeObject(domainEvent, SerializerSettings.Instance),
                OccurredAtUtc = domainEvent.OccurredAtUtc
            })
            .ToList();

        await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }
}
