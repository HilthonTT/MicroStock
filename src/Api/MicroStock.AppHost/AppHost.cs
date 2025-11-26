IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("Database")
    .WithDataVolume()
    .WithPgAdmin();

IResourceBuilder<RedisResource> redis = builder.AddRedis("Cache")
    .WithDataVolume();

var apiService = builder.AddProject<Projects.MicroStock_Api>("microstock-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.AddProject<Projects.MicroStock_Web>("microstock-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
