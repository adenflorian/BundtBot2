using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi.Controllers
{
    public class LogLevelObject
    {
        public LogLevel LogLevel { get; set; }
    }

    [Route("api/logs")]
    public class LogsApiController
    {
        [HttpPatch]
        public void ChangeLogLevel([FromBody]LogLevelObject loglevel)
        {
            if (loglevel == null) return;
            MyLogger.CurrentLogLevel = loglevel.LogLevel;
        }
    }
}