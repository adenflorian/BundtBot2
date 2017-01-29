using Microsoft.Extensions.Logging;

namespace BundtBot.WebApi
{
    public static class ILoggerFactoryExtensions
    {
        public static ILoggerFactory AddMyWebServerLogger(this ILoggerFactory factory)
        {
            factory.AddProvider(new MyLoggerProvider());
            return factory;
        }
    }
}