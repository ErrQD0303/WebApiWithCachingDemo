using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using WebApiWithCachingDemo.Repository;

using Microsoft.Extensions.Configuration;

/* DotNetEnv.Env.Load();
var redisServer = DotNetEnv.Env.GetString("REDIS_SERVER");
var redisUser = DotNetEnv.Env.GetString("USER") ?? "default";
var redisPassword = DotNetEnv.Env.GetString("PASSWORD"); */

Task RunApp(string url)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddUserSecrets<Program>();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(new Uri(url).Port, listenOptions =>
        {
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                listenOptions.UseHttps();
            }
        });
    });

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    AddCacheServices(builder);

    builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
    builder.Services.AddSingleton<CacheOptions>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    // app.Run();

    return app.RunAsync();
}

void AddCacheServices(WebApplicationBuilder builder)
{
    var cacheServer = builder.Configuration.GetValue<string>("CacheServer") ?? nameof(CacheType.InMemory);

    if (cacheServer == nameof(CacheType.InMemory))
    {
        builder.Services.AddSingleton<ICurrentTimeRepository, InMemoryCurrentTimeCacheRepository>(services =>
            new InMemoryCurrentTimeCacheRepository(
                new CurrentTimeRepository(
                    services.GetRequiredService<ILogger<CurrentTimeRepository>>()), services.GetRequiredService<IMemoryCache>(),
                    services.GetRequiredService<CacheOptions>(),
                    services.GetRequiredService<ILogger<InMemoryCurrentTimeCacheRepository>>()));
    }
    else if (cacheServer == nameof(CacheType.Redis))
    {
        builder.Services.AddSingleton<ICurrentTimeRepository, RedisCurrentTimeCacheRepository>(services =>
            new RedisCurrentTimeCacheRepository(
                new CurrentTimeRepository(services.GetRequiredService<ILogger<CurrentTimeRepository>>()),
                services.GetRequiredService<IDistributedCache>(),
                services.GetRequiredService<CacheOptions>(),
                services.GetRequiredService<ILogger<RedisCurrentTimeCacheRepository>>()));
    }

    switch (cacheServer)
    {
        case nameof(CacheType.InMemory):
            builder.Services.AddMemoryCache(options =>
            {
                options.SizeLimit = new CacheOptions().SizeLimit;
            });
            break;

        case nameof(CacheType.Redis):
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                var redisOptions = builder.Configuration.GetSection("REDIS").Get<RedisOptions>();
                if (redisOptions is null)
                {
                    throw new InvalidOperationException("RedisOptions is not found in the configuration");
                }
                options.Configuration = $"{redisOptions.SERVER_URL},user={redisOptions.USER},password={redisOptions.PASSWORD}";
                options.InstanceName = "ABC";
            });
            break;

        default:
            throw new InvalidOperationException("Invalid cache server");
    }

}

var task1 = RunApp("https://localhost:5000");
var task2 = RunApp("https://localhost:5001");

await Task.WhenAll(task1, task2);