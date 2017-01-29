using System;
using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi
{
    public class MyLoggerProvider : ILoggerProvider, IDisposable
    {
        private bool disposedValue = false;

        public ILogger CreateLogger(string categoryName)
        {
            return new MyWebServerLogger();
        }

        public void Dispose()
        {
        }
    }
}