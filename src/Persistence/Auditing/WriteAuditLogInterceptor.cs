using Application.Abstractions.Authentication;
using Application.Abstractions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;

namespace Persistence.Auditing;

internal sealed class WriteAuditLogInterceptor(IAuditingUserProvider auditingUserProvider) 
    : SaveChangesInterceptor
{
    private const string DefaultTableName = "Unknown Table";

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await WriteAuditLog(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task WriteAuditLog(
       DbContext context,
       CancellationToken cancellationToken)
    {
        IEnumerable<Audit> audits = CreateAudits(context);

        await context.AddRangeAsync(audits, cancellationToken);
    }

    private IEnumerable<Audit> CreateAudits(DbContext context)
    {
        string userId = auditingUserProvider.GetUserId();
        var auditEntries = new List<AuditEntry>();

        foreach (EntityEntry<Entity> entry in context.ChangeTracker.Entries<Entity>())
        {
            if (!entry.ShouldBeAudited())
            {
                continue;
            }

            string tableName = context
                .Model
                .FindEntityType(entry.Entity.GetType())?
                .GetTableName() ?? DefaultTableName;

            var auditEntry = new AuditEntry(
                entry,
                tableName,
                userId);

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (!property.IsAuditable())
                {
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                    case EntityState.Detached:
                        break;
                    case EntityState.Unchanged:
                        auditEntry.AuditType = AuditType.None;
                        break;
                    default:
                        throw new CustomException(
                            nameof(CreateAudits),
                            Error.Failure(
                                "AuditLog.Failure",
                                "Unable to determine entity state for audit log."));
                }
            }
        }

        return auditEntries.Select(auditEntry => auditEntry.ToAudit());
    }
}
