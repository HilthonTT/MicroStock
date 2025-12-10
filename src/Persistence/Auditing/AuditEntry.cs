using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Persistence.Auditing;

internal sealed class AuditEntry(EntityEntry entry, string tableName, string userId)
{
    public EntityEntry Entry { get; } = entry;

    public string UserId { get; set; } = userId;

    public string TableName { get; set; } = tableName;

    public Dictionary<string, object?> KeyValues { get; } = [];

    public Dictionary<string, object?> OldValues { get; } = [];

    public Dictionary<string, object?> NewValues { get; } = [];

    public AuditType AuditType { get; set; }

    public List<string> ChangedColumns { get; } = [];

    public Audit ToAudit()
    {
        Audit audit = Audit.Create(
            UserId,
            AuditType.ToString(),
            TableName,
            DateTime.UtcNow,
            JsonSerializer.Serialize(KeyValues),
            OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
            ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns));

        return audit;
    }
}
