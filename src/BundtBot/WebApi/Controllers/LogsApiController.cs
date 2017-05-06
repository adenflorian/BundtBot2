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
        static readonly MyLogger _logger = new MyLogger(nameof(LogsApiController), ConsoleColor.DarkBlue);

        [HttpPatch]
        public void ChangeLogLevel([FromBody]LogLevelObject loglevel)
        {
            if (loglevel == null) return;
            MyLogLevel.LogLevelOverride = loglevel.LogLevel;
            _logger.LogInfo("Log Level set to " + loglevel.LogLevel);
        }
    }
}