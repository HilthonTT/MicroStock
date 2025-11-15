using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Data;
using MicroStock.Common.Domain;
using MicroStock.Common.Infrastructure.Database;
using MicroStock.Common.Infrastructure.DomainEvents;
using MicroStock.Common.Infrastructure.Outbox;
using Quartz;
using System.Reflection;

namespace Modules.Users.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider,
    IDomainEventsDispatcher domainEventsDispatcher,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase(dbConnectionFactory, dateTimeProvider, domainEventsDispatcher, logger)
{
    protected override string ModuleName => "Users";

    protected override Assembly ApplicationAssembly => Application.AssemblyReference.Assembly;

    protected override string Schema => Schemas.Users;

    protected override int BatchSize => outboxOptions.Value.BatchSize;
}
