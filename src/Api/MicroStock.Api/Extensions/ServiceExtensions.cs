using Microsoft.OpenApi;
using MicroStock.Api.Middleware;
using MicroStock.Common.Application;
using MicroStock.Common.Infrastructure;
using Modules.Users.Infrastructure;

namespace MicroStock.Api.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    internal static IServiceCollection ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });
        });

        return services;
    }

    internal static IServiceCollection AddModules(
        this IServiceCollection services, 
        IConfiguration configuration,
        string databaseConnectionString,
        string cacheConnectionString)
    {
        services
           .AddApplication([
                Modules.Users.Application.AssemblyReference.Assembly
           ])
           .AddInfrastructure(
                configuration,
                [
                    UsersModule.ConfigureConsumers
                ],
               databaseConnectionString,
               cacheConnectionString);

        services.AddUsersModule(configuration);

        return services;
    }
}
