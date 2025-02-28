using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCaching;
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

        /* Output Caching */
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
        [HttpGet("EvictedEndpoint")]
        [OutputCache(Tags = ["tag-blog"])] // This attribute is used to cache the response of the action method
        public IActionResult GetOutputByTagBlog()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            var etag = $"\"{Guid.NewGuid():n}\"";
            Response.Headers.ETag = etag; // Set an ETag header to the client browser, so that the client can send it back to the server in the If-None-Match header, if the etag is still valid, the server will return 304 Not Modified status code
            return Ok(formattedTime);
        }

        [HttpPost("EvictCache/{tag}")] // This action method is used to evict the cache by tag to all client browsers
        public IActionResult EvictAllCacheByTag(string tag = "tag-blog")
        {
            _cacheStore.EvictByTagAsync(tag, default);
            return Ok("Cache evicted");
        }

        /* Response Caching */
        [HttpGet("ResponseCache")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 30)] // This attribute is used to cache the response of the action method
        // Response
        // Cache-Control: public, max-age=30 // The cache may store in shared cache or private cache and has a maximum age of 30 seconds
        // Vary: User-Agent // If you are using different User-Agent (a.k.a different browser), the cache will be different
        public IActionResult GetResponseCache()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [HttpGet("ResponseCacheNoStore")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)] // This attribute is used to cache the response of the action method
        // Response
        // Cache-Control: no-store, no-cache
        // Pragma: no-cache
        // Explains:
        // NoStore = true set the Cache-Control header to no-store
        // Location = ResponseCacheLocation.None set the Pragma header to no-cache and add to the Cache-Control header no-cache, else the Cache-Control header will be set to private or public, and the Pragma header will not be added
        public IActionResult GetResponseCacheNoStore()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [HttpGet("ResponseCacheLocationAnyWith10secondsDuration")]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
        // Response
        // Cache-Control: public, max-age=10
        public IActionResult GetResponseCacheLocationAnyWith10secondsDuration()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [HttpGet("WithDefault30CacheProfile")]
        [ResponseCache(CacheProfileName = "Default30")]
        // Response
        // Cache-Control: public, max-age=10
        public IActionResult GetDefault30CacheProfile()
        {
            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("WithResponseCachingFeature")]
        public IActionResult GetResponseCachingFeature([FromQuery] string? name)
        {
            var responseCacheFeature = HttpContext.Features.Get<IResponseCachingFeature>();

            if (responseCacheFeature is not null)
            {
                responseCacheFeature.VaryByQueryKeys = new[] { "name" };
            }

            // With each different query string, the cache will be different
            // If a new value of the query string is set, the old cache will not be evicted unless the storage size limit is reached

            return Content(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $" - {name}");
        }
    }
}