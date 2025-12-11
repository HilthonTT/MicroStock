using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Persistence.Database;
using Persistence.Serialization;
using Quartz;
using SharedKernel;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IDomainEventsDispatcher domainEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    ILogger<ProcessOutboxJob> logger,
    IOptions<OutboxOptions> options) : IJob
{
    private readonly OutboxOptions _options = options.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Beginning to process outbox messages");

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(context.CancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(context.CancellationToken);

        IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetUnprocessedOutboxMessagesAsync(connection, transaction);

        if (outboxMessages.Count == 0)
        {
            logger.LogInformation("No outbox messages to process");
            return;
        }

        logger.LogInformation("Processing {Count} outbox messages", outboxMessages.Count);

        int successCount = 0;
        int failureCount = 0;

        foreach (OutboxMessageResponse outboxMessage in outboxMessages)
        {
            Exception? exception = null;
            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content, 
                    SerializerSettings.Instance)!;

                await domainEventsDispatcher.DispatchAsync(domainEvent, context.CancellationToken);
                successCount++;
            }
            catch (Exception caughtException)
            {
                logger.LogError(caughtException, "Exception while processing outbox message {MessageId}", outboxMessage.Id);

                exception = caughtException;
                failureCount++;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
        }

        await transaction.CommitAsync(context.CancellationToken);

        logger.LogInformation(
            "Completed processing outbox messages. Success: {SuccessCount}, Failed: {FailureCount}",
            successCount,
            failureCount);
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetUnprocessedOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        string sql =
            $"""
             SELECT
                id AS {nameof(OutboxMessageResponse.Id)},
                content AS {nameof(OutboxMessageResponse.Content)}
             FROM {Schemas.Application}.outbox_messages
             WHERE processed_at_utc IS NULL
             ORDER BY occurred_at_utc
             LIMIT {_options.BatchSize}
             FOR UPDATE SKIP LOCKED
             """;

        IEnumerable<OutboxMessageResponse> outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(
            sql, 
            transaction: transaction);

        return outboxMessages.ToList();
    }

    private async Task UpdateOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxMessageResponse outboxMessage,
        Exception? exception)
    {
        const string sql =
            $"""
            UPDATE {Schemas.Application}.outbox_messages
            SET processed_at_utc = @ProcessedAtUtc,
                error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                outboxMessage.Id,
                ProcessedAtUtc = dateTimeProvider.UtcNow,
                Error = exception?.ToString(),
            },
            transaction: transaction);
    }

    private sealed record OutboxMessageResponse(Guid Id, string Content);
}
