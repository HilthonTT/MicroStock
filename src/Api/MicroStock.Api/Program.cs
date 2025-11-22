using MicroStock.Api.Extensions;
using MicroStock.Common.Presentation.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddLogging();

builder.Configuration.AddModuleConfiguration([
    "users"
]);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
string cacheConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache");

builder.Services
    .AddExceptionHandling()
    .ConfigureOpenApi()
    .AddModules(builder.Configuration, databaseConnectionString, cacheConnectionString);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();
}

app.MapEndpoints();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseUserContextEnrichment();
app.UseETag();

await app.RunAsync();
