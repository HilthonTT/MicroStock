using Application.Abstractions.Data;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Persistence.Auditing;
using Persistence.Data;
using Persistence.Database;
using Persistence.Outbox;
using Persistence.Repositories;

namespace Persistence;

public static class PersistenceConfiguration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddAuditing();

        var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.AddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.AddSingleton<InsertOutboxMessagesInterceptor>();

        services.AddDbContext<ApplicationDbContext>(Postgres.StandardOptions(databaseConnectionString, Schemas.Application));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
