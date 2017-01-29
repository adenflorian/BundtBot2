using System;
using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi
{
    public class MyLoggerProvider : ILoggerProvider, IDisposable
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyWebServerLogger();
        }

        public void Dispose()
        {
        }
    }
}