using Application.Abstractions.EventBus;
using Application.Abstractions.Messaging;
using Infrastructure.Authentication;
using Infrastructure.Cors;
using Infrastructure.Inbox;
using Infrastructure.Outbox;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Database;
using Quartz;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

namespace Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string cacheConnectionString)
    {
        services
            .AddAuthenticationServices()
            .AddBackgroundJobs(configuration)
            .AddEventBus()
            .AddCaching(cacheConnectionString)
            .AddDomainEventHandlers()
            .AddCorsPolicy(configuration)
            .ConfigureRateLimiting();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        services.ConfigureOptions<ConfigureJwtAuthOption>();

        using var serviceProvider = services.BuildServiceProvider();
        JwtAuthOptions jwtAuthOptions = serviceProvider.GetRequiredService<IOptions<JwtAuthOptions>>().Value;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtAuthOptions.Issuer,
                ValidAudience = jwtAuthOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
            };
        });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(configurator =>
        {
            Guid scheduler = Guid.CreateVersion7();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.ConfigurationSectionName));
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSectionName));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
        services.ConfigureOptions<ConfigureProcessInboxJob>();

        return services;
    }

    private static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus.EventBus>();

        services.AddMassTransit(configurator =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumer in ConfigureConsumers())
            {
                configureConsumer(configurator);
            }

            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, string cacheConnectionString)
    {
        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(cacheConnectionString);
            services.AddSingleton(connectionMultiplexer);

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
            });
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    private static IEnumerable<Action<IRegistrationConfigurator>> ConfigureConsumers()
    {
        List<Type> consumerTypes = Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToList();

        foreach (Type integrationEventHandlerType in consumerTypes)
        {
            Type integrationEventType = integrationEventHandlerType
                .GetInterfaces()
                .Single(@interface => @interface.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type consumerType = typeof(IntegrationEventConsumer<>).MakeGenericType(integrationEventType);

            yield return configurator => configurator.AddConsumer(consumerType);
        }
    }

    private static IServiceCollection AddDomainEventHandlers(this IServiceCollection services)
    {
        List<Type> domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToList();

        foreach (Type domainEventHandlerType in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandlerType);

            Type domainEventType = domainEventHandlerType
                .GetInterfaces()
                .Single(@interface => @interface.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandlerType = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEventType);

            services.Decorate(domainEventHandlerType, closedIdempotentHandlerType);
        }

        return services;
    }

    private static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests, "Too Many Requests",
                        detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.");
                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
                }
            };

            options.AddPolicy("default", context =>
            {
                string userName = context.User.GetIdentityId() ?? string.Empty;
                if (!string.IsNullOrEmpty(userName))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(userName, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 100,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 25
                        });
                }

                return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });

        return services;
    }

    private static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        CorsOptions corsOptions = configuration.GetSection(CorsOptions.ConfigurationSectionName).Get<CorsOptions>()!;

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
