using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiWithCachingDemo.Repository
{
    public class RedisOptions
    {
        public string SERVER_URL { get; set; } = string.Empty;
        public string INSTANCE_NAME { get; set; } = string.Empty;
        public string USER { get; set; } = "default";
        public string PASSWORD { get; set; } = string.Empty;
    }
}