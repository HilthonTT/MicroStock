using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Auditing;
using System.Reflection;

namespace Persistence.Auditing;

internal static class AuditingExtensions
{
    internal static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<WriteAuditLogInterceptor>();

        return services;
    }

    internal static bool ShouldBeAudited(this EntityEntry entry) =>
        entry.State != EntityState.Detached &&
        entry.State != EntityState.Unchanged &&
        entry.Entity is not Audit &&
        entry.IsAuditable();

    internal static bool IsAuditable(this EntityEntry entityEntry)
    {
        AuditableAttribute? entityAuditableAttribute = Attribute.GetCustomAttribute(
            entityEntry.Entity.GetType(),
            typeof(AuditableAttribute)) as AuditableAttribute;

        return entityAuditableAttribute is not null;
    }

    internal static bool IsAuditable(this PropertyEntry propertyEntry)
    {
        Type entityType = propertyEntry.EntityEntry.Entity.GetType();
        PropertyInfo propertyInfo = entityType.GetProperty(propertyEntry.Metadata.Name)!;
        bool propertyAuditingDisabled = Attribute.IsDefined(propertyInfo, typeof(NotAuditableAttribute));

        return IsAuditable(propertyEntry.EntityEntry) && !propertyAuditingDisabled;
    }
}
