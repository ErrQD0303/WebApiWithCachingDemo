using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace WebApiWithCachingDemo.Repository
{
    public class RedisCurrentTimeCacheRepository : ICurrentTimeRepository
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ICurrentTimeRepository _parentRepository;
        private readonly CacheOptions _options;
        private readonly ILogger<RedisCurrentTimeCacheRepository> _logger;

        public RedisCurrentTimeCacheRepository(ICurrentTimeRepository parentRepository, IDistributedCache distributedCache, CacheOptions options, ILogger<RedisCurrentTimeCacheRepository> logger)
        {
            _parentRepository = parentRepository;
            _cache = distributedCache;
            _options = options;
            _logger = logger;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.AbsoluteExpirationRelativeToNow),
                SlidingExpiration = TimeSpan.FromSeconds(options.SlidingExpiration),
            };
        }

        // Cache-Aside Strategy
        public DateTime GetCurrentTime()
        {
            var currentTimeByte = _cache.Get(_options.CacheKey);
            if (currentTimeByte is null)
            {
                var parentCurrentTime = _parentRepository.GetCurrentTime();
                currentTimeByte = BitConverter.GetBytes(parentCurrentTime.ToBinary());
                _cache.Set(_options.CacheKey, currentTimeByte, _cacheOptions);
            }

            var currentTime = DateTime.FromBinary(BitConverter.ToInt64(currentTimeByte, 0));

            return currentTime;
        }
    }
}