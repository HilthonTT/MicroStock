using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicroStock.Common.Application.Caching;
using MicroStock.Common.Application.Data;
using MicroStock.Common.Application.EventBus;
using MicroStock.Common.Domain;
using MicroStock.Common.Infrastructure.Auditing;
using MicroStock.Common.Infrastructure.Authentication;
using MicroStock.Common.Infrastructure.Authorization;
using MicroStock.Common.Infrastructure.Caching;
using MicroStock.Common.Infrastructure.Cors;
using MicroStock.Common.Infrastructure.Data;
using MicroStock.Common.Infrastructure.Origin;
using MicroStock.Common.Infrastructure.Outbox;
using MicroStock.Common.Infrastructure.Time;
using Npgsql;
using Quartz;
using StackExchange.Redis;

namespace MicroStock.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
        string databaseConnectionString,
        string cacheConnectionString)
    {
        services.AddAuthorizationInternal();
        services.AddAuthenticationInternal();
        services.AddCorsInternal();

        services.AddAuditing();

        var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        services.TryAddSingleton<WriteAuditLogInterceptor>();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddQuartz(configurator =>
        {
            Guid scheduler = Guid.CreateVersion7();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(cacheConnectionString);
            services.TryAddSingleton(connectionMultiplexer);

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
            });
        }
        catch
        {
            // HACK: Allows application to run without a Redis server. This is useful when, for example, generating a database migration.
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, CacheService>();
        services.TryAddSingleton<IEventBus, EventBus.EventBus>();

        services.AddMassTransit(configurator =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumer in moduleConfigureConsumers)
            {
                configureConsumer(configurator);
            }

            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });

        services.ConfigureOptions<CorsConfigureOptions>();
        services.ConfigureOptions<OriginConfigureOptions>();

        return services;
    }
}
