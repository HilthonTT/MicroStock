using Application.Abstractions.Data;
using Application.Abstractions.EventBus;
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

namespace Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IIntegrationEventsDispatcher integrationEventsDispatcher,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> options,
    ILogger<ProcessInboxJob> logger) : IJob
{
    private readonly InboxOptions _options = options.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Beginning to process inbox messages");

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(context.CancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(context.CancellationToken);

        IReadOnlyList<InboxMessageResponse> inboxMessages = await GetUnprocessedInboxMessagesAsync(connection, transaction);

        foreach (InboxMessageResponse inboxMessage in inboxMessages)
        {
            Exception? exception = null;
            try
            {
                IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
                    inboxMessage.Content, 
                    SerializerSettings.Instance)!;

                await integrationEventsDispatcher.DispatchAsync(integrationEvent, context.CancellationToken);
            }
            catch (Exception caughtException)
            {
                logger.LogError(caughtException, "Exception while processing inbox message {MessageId}", inboxMessage.Id);

                exception = caughtException;
            }

            await UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
        }

        await transaction.CommitAsync(context.CancellationToken);

        logger.LogInformation("Completed processing inbox messages");
    }


    private async Task<IReadOnlyList<InboxMessageResponse>> GetUnprocessedInboxMessagesAsync(
      IDbConnection connection,
      IDbTransaction transaction)
    {
        string sql =
            $"""
             SELECT
                id AS {nameof(InboxMessageResponse.Id)},
                content AS {nameof(InboxMessageResponse.Content)}
             FROM {Schemas.Application}.inbox_messages
             WHERE processed_at_utc IS NULL
             ORDER BY occurred_at_utc
             LIMIT {_options.BatchSize}
             FOR UPDATE
             """;

        var inboxMessages = await connection.QueryAsync<InboxMessageResponse>(sql, transaction: transaction);
        return inboxMessages.ToList();
    }

    private async Task UpdateInboxMessageAsync(
       IDbConnection connection,
       IDbTransaction transaction,
       InboxMessageResponse inboxMessage,
       Exception? exception)
    {
        const string sql =
            $"""
            UPDATE {Schemas.Application}.inbox_messages
            SET processed_at_utc = @ProcessedAtUtc,
               error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                inboxMessage.Id,
                ProcessedAtUtc = dateTimeProvider.UtcNow,
                Error = exception?.Message
            }, transaction: transaction);
    }

    private sealed record InboxMessageResponse(Guid Id, string Content);
}
