using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiWithCachingDemo.Repository
{
    public class CurrentTimeRepository : ICurrentTimeRepository
    {
        private readonly ILogger<CurrentTimeRepository> _logger;

        public CurrentTimeRepository(ILogger<CurrentTimeRepository> logger)
        {
            _logger = logger;
        }

        public DateTime GetCurrentTime()
        {
            var currentTime = DateTime.Now;
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            _logger.LogInformation($"Current time is {formattedTime}");
            return currentTime;
        }
    }
}