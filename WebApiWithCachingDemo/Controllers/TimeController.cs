using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using WebApiWithCachingDemo.Repository;

namespace WebApiWithCachingDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        private readonly ILogger<TimeController> _logger;
        private readonly IUnitOfWork _uOW;
        private readonly IOutputCacheStore _cacheStore;

        public TimeController(ILogger<TimeController> logger, IUnitOfWork unitOfWork, IOutputCacheStore cacheStore)
        {
            _logger = logger;
            _uOW = unitOfWork;
            _cacheStore = cacheStore;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            return Ok(formattedTime);
        }

        [HttpGet("ExpireDefault")]
        [OutputCache] // This attribute is used to cache the response of the action method
        public IActionResult GetOutputExpireDefault()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            var etag = $"\"{Guid.NewGuid():n}\"";
            Response.Headers.ETag = etag; // Set an ETag header to the client browser, so that the client can send it back to the server in the If-None-Match header, if the etag is still valid, the server will return 304 Not Modified status code
            return Ok(formattedTime);
        }

        [HttpGet("ExpireAfterTen")]
        [OutputCache(PolicyName = "Expire10")] // This attribute is used to cache the response of the action method
        public IActionResult GetOutputExpireAfterTen()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            var etag = $"\"{Guid.NewGuid():n}\"";
            Response.Headers.ETag = etag; // Set an ETag header to the client browser, so that the client can send it back to the server in the If-None-Match header, if the etag is still valid, the server will return 304 Not Modified status code
            return Ok(formattedTime);
        }

        // This endpoint is used to cache the response for default 20 seconds, and use the Policy EvictTagBlog to cache the response by tag "tag-blog"
        [HttpGet("EvictEndpoint")]
        [OutputCache(PolicyName = "EvictTagBlog", Tags = ["tag-blog"])] // This attribute is used to cache the response of the action method
        public IActionResult GetOutputByTagBlog()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            var etag = $"\"{Guid.NewGuid():n}\"";
            Response.Headers.ETag = etag; // Set an ETag header to the client browser, so that the client can send it back to the server in the If-None-Match header, if the etag is still valid, the server will return 304 Not Modified status code
            return Ok(formattedTime);
        }

        [HttpPost("EvictCache/{tag}")] // This action method is used to evict the cache by tag to all client browsers
        public IActionResult EvictAllCacheByTag(string tag)
        {
            _cacheStore.EvictByTagAsync(tag, default);
            return Ok("Cache evicted");
        }
    }
}