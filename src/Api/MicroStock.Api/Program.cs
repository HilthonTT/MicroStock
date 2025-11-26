using MicroStock.Api.Extensions;
using MicroStock.Common.Presentation.Endpoints;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();

    await app.ApplyMigrations();
    await app.SeedInitialDataAsync();
}


app.UseCors();

app.MapEndpoints();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseUserContextEnrichment();
app.UseETag();

app.UseStatusCodePages();

await app.RunAsync();
