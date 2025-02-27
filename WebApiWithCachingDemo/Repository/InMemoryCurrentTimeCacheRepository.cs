using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace WebApiWithCachingDemo.Repository
{
    public class InMemoryCurrentTimeCacheRepository : ICurrentTimeRepository
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly ICurrentTimeRepository _parentRepository;
        private readonly CacheOptions _options;
        private readonly ILogger<InMemoryCurrentTimeCacheRepository> _logger;

        public InMemoryCurrentTimeCacheRepository(ICurrentTimeRepository parentRepository, IMemoryCache memoryCache, CacheOptions options, ILogger<InMemoryCurrentTimeCacheRepository> logger)
        {
            _parentRepository = parentRepository;
            _cache = memoryCache;
            _options = options;
            _logger = logger;
            _cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.AbsoluteExpirationRelativeToNow),
                SlidingExpiration = TimeSpan.FromSeconds(options.SlidingExpiration),
                Size = 1
            }
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogInformation($"Cache entry with key {key} has been evicted due to {reason}");
            });
        }

        // Cache-Aside Strategy
        public DateTime GetCurrentTime()
        {
            // Use this line to simulate a long-running operation and run async code
            // In real life code, you should pass the CancellationToken to the method
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            // Trigger cancellation after 5 seconds
            // Solution 1
            /* if (!_cache.TryGetValue(_options.CacheKey, out DateTime currentTime))
            {
                currentTime = _parentRepository.GetCurrentTime();
                _cache.Set(_options.CacheKey, currentTime, _cacheOptions);
            } */
            // Solution 2
            var currentTimeByte = _cache.GetOrCreate(
                _options.CacheKey,
                entry =>
                {
                    var time = _parentRepository.GetCurrentTime();
                    var timeBytes = BitConverter.GetBytes(time.ToBinary());
                    return timeBytes;
                },
                _cacheOptions
            /* .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token)) // The cache entry will be removed when the token is expired, which means after 5 seconds after the set operation
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogInformation($"Cache entry with key {key} has been evicted due to {reason}");
                ((CancellationTokenSource)state!).Dispose();
            }, cancellationTokenSource) */
            ) ?? throw new Exception("Invalid cache value");

            var currentTime = DateTime.FromBinary(BitConverter.ToInt64(currentTimeByte, 0));

            return currentTime;
        }
    }
}