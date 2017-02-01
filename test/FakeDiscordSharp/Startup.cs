using System;
using System.Threading.Tasks;
using BundtCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
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

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            reset = UnixTime.GetTimestamp() + 5;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay((reset - UnixTime.GetTimestamp()) * 1000);
                    remaining = limit;
                    reset = UnixTime.GetTimestamp() + offset;
                }
            });
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            app.UseStaticFiles();

            //var _resetFakeOffset = (int)app.Properties["_resetFakeOffset"];

            //var IncrementRateLimitExceededCount = app.Properties["IncrementRateLimitExceededCount"] as Action;

            app.Run(async (context) =>
            {
                remaining--;

                context.Response.Headers.Clear();
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = (reset + FakeDiscord._resetFakeOffset).ToString();

                if (remaining < 0)
                {
                    //IncrementRateLimitExceededCount.Invoke();
                    FakeDiscord.RateLimitExceededCount++;
                    context.Response.StatusCode = 429;
                    await context.Response
                        .WriteAsync("{\"message\":\"You are being rate limited.\",\"retry_after\": 6457,\"global\": false}");
                }
                else
                {
                    await context.Response
                        .WriteAsync("{\"id\": \"162701077035089920\",\"channel_id\": \"131391742183342080\",\"author\": {},\"content\": \"Hey guys!\",\"timestamp\": \"2016-03-24T23:15:59.605000+00:00\",\"edited_timestamp\": null,\"tts\": false,\"mention_everyone\": false,\"mentions\": [],\"mention_roles\": [],\"attachments\": [],\"embeds\": [],\"reactions\": []}");
                }

            });
        }
    }
}