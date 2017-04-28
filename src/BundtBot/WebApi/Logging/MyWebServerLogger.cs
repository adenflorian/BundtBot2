using System;
using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi
{
    public class MyWebServerLogger : ILogger
    {
        static readonly MyLogger _logger = new MyLogger(nameof(MyWebServerLogger), ConsoleColor.Blue);

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }
        
        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message)) return;
            
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Debug:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Information:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    if (exception != null) _logger.LogError(exception);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(message);
                    if (exception != null) _logger.LogCritical(exception);
                    break;
                case LogLevel.None:
                    break;
            }
        }
    }
}