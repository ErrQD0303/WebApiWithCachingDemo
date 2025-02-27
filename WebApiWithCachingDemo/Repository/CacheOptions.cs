namespace WebApiWithCachingDemo.Repository
{
    public class CacheOptions
    {
        public string CacheKey { get; init; } = "CurrentTime";
        public long AbsoluteExpirationRelativeToNow { get; init; } = 15;
        public long SlidingExpiration { get; init; } = 3;
        public int SizeLimit { get; init; } = 10; // maximum 10 units
        public int SetSize { get; init; } = 1; // Each set operation will add 1 unit
        public double CompactionPercentage { get; init; } = 0.2; // 20% of the cache size
    }
}