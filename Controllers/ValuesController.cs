using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace WebApi_AWS_Starter.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ILogger<ValuesController> _log;
        private IDistributedCache _distributedCache;
        public ValuesController(ILogger<ValuesController> log, IDistributedCache distributedCache)
        {
            _log=log;
            _distributedCache=distributedCache;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var cacheKey = "TheTime";
            var currentTime = _distributedCache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(currentTime))
            {
                return "Fetched from cache : " + currentTime;
            }
            else
            {
                currentTime = DateTime.UtcNow.ToString();
                _distributedCache.SetString(cacheKey, currentTime);
                return "Added to cache : " + currentTime;
            }
        }

        // GET api/values
        // [HttpGet]
        // public IEnumerable<string> Get()
        // {
        //     _log.LogInformation("GET api/values");
        //     return new string[] { "value1", "value2" };
        // }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
