using Microsoft.Extensions.Logging;

namespace BundtBot
{
    public class MyLogLevel
    {
        public static LogLevel? LogLevelOverride = null;

        LogLevel _currentLogLevel;
        public LogLevel CurrentLogLevel
        {
            get
            {
                return LogLevelOverride.HasValue ? LogLevelOverride.Value : _currentLogLevel;
            }
            set
            {
                _currentLogLevel = value;
            }
        }
    }
}