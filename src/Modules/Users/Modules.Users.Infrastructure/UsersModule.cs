using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicroStock.Common.Application.Authentication;
using MicroStock.Common.Application.EventBus;
using MicroStock.Common.Application.Messaging;
using MicroStock.Common.Infrastructure.Database;
using MicroStock.Common.Presentation.Endpoints;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Authentication;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Inbox;
using Modules.Users.Infrastructure.Outbox;
using Modules.Users.Infrastructure.Repositories;

namespace Modules.Users.Infrastructure;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();

        services
           .AddInfrastructure(configuration)
           .AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityProvider(configuration);
        services.AddDatabase(configuration);
        services.AddOutbox(configuration);
        services.AddInbox(configuration);

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContext<UsersDbContext>(Postgres.StandardOptions(configuration, Schemas.Users))
            .AddScoped<IDbContext>(sp => sp.GetRequiredService<UsersDbContext>())
            .AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddIdentityProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<UsersIdentityDbContext>();

        services
            .AddDbContext<UsersIdentityDbContext>(Postgres.IdentityOptions(configuration, Schemas.Identity))
            .AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<UsersIdentityDbContext>());

        services.AddScoped<IUserContext, UserContext>();
        services.AddTransient<ITokenProvider, TokenProvider>();

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator) =>
        Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToList()
            .ForEach(integrationEventHandlerType =>
            {
                Type integrationEventType = integrationEventHandlerType
                    .GetInterfaces()
                    .Single(@interface => @interface.IsGenericType)
                    .GetGenericArguments()
                    .Single();

                registrationConfigurator.AddConsumer(typeof(IntegrationEventConsumer<>).MakeGenericType(integrationEventType));
            });

    private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration) =>
       services
           .Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"))
           .ConfigureOptions<ConfigureProcessOutboxJob>();

    private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration) =>
        services
            .Configure<InboxOptions>(configuration.GetSection("Users:Inbox"))
            .ConfigureOptions<ConfigureProcessInboxJob>();

    private static void AddDomainEventHandlers(this IServiceCollection services) =>
        Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToList()
            .ForEach(domainEventHandlerType =>
            {
                services.TryAddScoped(domainEventHandlerType);

                Type domainEventType = domainEventHandlerType
                    .GetInterfaces()
                    .Single(@interface => @interface.IsGenericType)
                    .GetGenericArguments()
                    .Single();

                Type closedIdempotentHandlerType = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEventType);

                services.Decorate(domainEventHandlerType, closedIdempotentHandlerType);
            });

    private static void AddIntegrationEventHandlers(this IServiceCollection services) =>
        Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToList()
            .ForEach(integrationEventHandlerType =>
            {
                services.TryAddScoped(integrationEventHandlerType);

                Type integrationEventType = integrationEventHandlerType
                    .GetInterfaces()
                    .Single(@interface => @interface.IsGenericType)
                    .GetGenericArguments()
                    .Single();

                Type closedIdempotentHandlerType = typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEventType);

                services.Decorate(integrationEventHandlerType, closedIdempotentHandlerType);
            });
}
