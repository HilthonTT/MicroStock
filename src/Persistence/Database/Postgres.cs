using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Auditing;
using Persistence.Outbox;

namespace Persistence.Database;

internal static class Postgres
{
    public static Action<IServiceProvider, DbContextOptionsBuilder> StandardOptions(string databaseConnectionString, string schema) =>
       (serviceProvider, options) =>
       {
           options.UseNpgsql(
                   databaseConnectionString,
                   optionsBuilder =>
                   {
                       optionsBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema);
                   }).UseSnakeCaseNamingConvention()
               .AddInterceptors(
                   serviceProvider.GetRequiredService<InsertOutboxMessagesInterceptor>(),
                   serviceProvider.GetRequiredService<WriteAuditLogInterceptor>());
       };
}
