using Dapper;
using MassTransit;
using MicroStock.Common.Application.Data;
using MicroStock.Common.Application.EventBus;
using MicroStock.Common.Infrastructure.Inbox;
using MicroStock.Common.Infrastructure.Serialization;
using Newtonsoft.Json;
using System.Data.Common;

namespace Modules.Users.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(IDbConnectionFactory dbConnectionFactory)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var integrationEvent = context.Message;

        var inboxMessage = new InboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            OccurredAtUtc = integrationEvent.OccurredAtUtc,
        };

        const string sql =
           """
            INSERT INTO users.inbox_messages(id, type, content, occurred_at_utc)
            VALUES (@Id, @Type, @Content::json, @OccurredAtUtc);
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
