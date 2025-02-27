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

    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(6)));
        options.AddPolicy("Expire10", builder =>
            builder.Expire(TimeSpan.FromSeconds(10)));
        options.AddPolicy("Expire20", builder => builder.Expire(TimeSpan.FromSeconds(20)));
        options.AddPolicy("Query", builder => builder.SetVaryByQuery("culture")); // Only cache based on the query parameter
        options.AddPolicy("Header", builder => builder.SetVaryByHeader("Accept-Language")); // Only cache based on the header
        options.AddPolicy("HostHeader", builder => builder.SetVaryByHost(true)); // Only cache based on the Host Header
        options.AddPolicy("NoCache", builder => builder.NoCache()); // No cache
        options.AddPolicy("NoLock", builder => builder.SetLocking(false)); // No locking mechanism for the cache item, The Resource Locking is mainly used to prevent the cache stampede problem
        options.AddPolicy("EvictTagBlog", builder => builder
        .With(c => c.HttpContext.Request.Path.StartsWithSegments("/api/time"))
        .Tag("tag-blog"));

        /* // Set the size limit for the output cache
        options.SizeLimit = 100; // Maximum size of cache storage. THe default value is 100 MB. When this limit is reached, no new cache item will be added to the cache storage until some cache items are removed from the cache storage. So you can use the cache eviction endpoints which use IOutputCacheStore to remove the cache items from the cache storage
        options.MaximumBodySize = 64; // IF the response body exceeds this size, the response body will not be cached
        options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60); // Default expiration time span for the cache item when not specified by the OutputCache policy. The default value is 60 seconds */

        // The Output Cache will be stored in the cache service which is registered in the DI container, by default, the InMemory cache service is used, if you want to use the Redis cache service, you need to register the Redis cache service in the DI container using normal Redis cache service registration
    });

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
    app.UseOutputCache(); // Output Cache middleware should be added after UseCors

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