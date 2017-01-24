
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FakeDiscordSharp
{
    public class Startup
    {
        const int limit = 5;
        const int offset = 5;

        int remaining = 5;
        int reset;

        int getTimestamp()
        {
            return (int)Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            reset = getTimestamp() + 5;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay((reset - getTimestamp()) * 1000);
                    remaining = limit;
                    reset = getTimestamp() + offset;
                }
            });
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            app.UseStaticFiles();

            app.Run(async (context) =>
            {
                var warning = "";
                if (remaining <= 0)
                {
                    warning = "RATE LIMIT EXCEEDED! ";
                }
                remaining--;

                context.Response.Headers.Clear();
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = reset.ToString();
                await context.Response
                    .WriteAsync($"{warning}Now: {getTimestamp()} Limit: {limit} | Remaining: {remaining} | Reset: {reset}");
            });
        }
    }
}