using Microsoft.Extensions.Caching.Memory;
using WebApiWithCachingDemo.Repository;

Task RunApp(string url)
{
    var builder = WebApplication.CreateBuilder(args);

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

    builder.Services.AddMemoryCache(options =>
    {
        options.SizeLimit = new CacheOptions().SizeLimit;
    });

    builder.Services.AddSingleton<ICurrentTimeRepository, InMemoryCurrentTimeCacheRepository>(services => new InMemoryCurrentTimeCacheRepository(new CurrentTimeRepository(services.GetRequiredService<ILogger<CurrentTimeRepository>>()), services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<CacheOptions>(), services.GetRequiredService<ILogger<InMemoryCurrentTimeCacheRepository>>()));
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

var task1 = RunApp("https://localhost:5000");
var task2 = RunApp("https://localhost:5001");

await Task.WhenAll(task1, task2);