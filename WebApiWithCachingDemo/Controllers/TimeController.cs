using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApiWithCachingDemo.Repository;

namespace WebApiWithCachingDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        private readonly ILogger<TimeController> _logger;
        private readonly IUnitOfWork _uOW;

        public TimeController(ILogger<TimeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _uOW = unitOfWork;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var currentTime = _uOW.CurrentTimeRepository.GetCurrentTime();
            var formattedTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            return Ok(formattedTime);
        }
    }
}