using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Persistence.Database;
using Persistence.Outbox;
using SharedKernel;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory) : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);

        if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer))
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);

        await InsertOutboxConsumerAsync(connection, outboxMessageConsumer);
    }

    private static async Task<bool> OutboxConsumerExistsAsync(
        IDbConnection connection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        const string sql =
           $"""
            SELECT EXISTS(
                SELECT 1
                FROM {Schemas.Application}.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId AND 
                      name = @Name
            )
            """;

        return await connection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
    }

    private static async Task InsertOutboxConsumerAsync(
        IDbConnection connection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        const string sql =
            $"""
            INSERT INTO {Schemas.Application}.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;

        await connection.ExecuteAsync(sql, outboxMessageConsumer);
    }
}
