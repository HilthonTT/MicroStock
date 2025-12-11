using Application.Abstractions.Data;
using Application.Abstractions.EventBus;
using Dapper;
using MassTransit;
using Newtonsoft.Json;
using Persistence.Database;
using Persistence.Inbox;
using Persistence.Serialization;
using System.Data.Common;

namespace Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(context.CancellationToken);

        TIntegrationEvent integrationEvent = context.Message;

        var inboxMessage = new InboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            OccurredAtUtc = integrationEvent.OccurredAtUtc
        };

        const string sql =
            $"""
            INSERT INTO {Schemas.Application}.inbox_messages(id, type, content, occurred_at_utc)
            VALUES (@Id, @Type, @Content::json, @OccurredAtUtc);
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
