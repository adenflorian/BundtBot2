using System;
using System.Threading.Tasks;
using BundtBot;
using BundtCommon.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FakeDiscordSharp
{
    public class FakeDiscordServer
    {
        static readonly MyLogger _logger = new MyLogger(nameof(FakeDiscordServer), ConsoleColor.Red);
        const int limit = 5;
        readonly TimeSpan _resetOffset = TimeSpan.FromSeconds(5);
        readonly TimeSpan _serverTimeOffset = TimeSpan.FromSeconds(2);

        int remaining = 5;
        DateTime reset;

        public FakeDiscordServer(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            reset = CalculateResetTime();

            Task.Run(async () =>
            {
                while (true)
                {
                    var timeToWait = reset - GetServerTime();
                    _logger.LogInfo($"Waiting {(int)timeToWait.TotalSeconds} seconds until reset");
                    await Task.Delay(timeToWait);
                    remaining = limit;
                    reset = CalculateResetTime();
                    _logger.LogInfo("Reset time reset to " + reset.ToString("hh:mm:ss.fff"));
                }
            });
        }

        DateTime CalculateResetTime()
        {
            return GetServerTime().AddSeconds(_resetOffset.TotalSeconds);
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            app.UseStaticFiles();

            app.Run(async (context) =>
            {
                _logger.LogInfo("Request started!");
                _logger.LogDebug($"{nameof(GetServerTime)}: {GetServerTime().ToString("hh:mm:ss.fff")}");
                _logger.LogDebug($"{nameof(reset)}: {reset.ToString("hh:mm:ss.fff")}");
                var timeUntilReset = reset - GetServerTime();
                _logger.LogDebug($"{nameof(timeUntilReset)}: {timeUntilReset.TotalSeconds} seconds");
                _logger.LogDebug($"{nameof(_resetOffset)}: {_resetOffset}");
                _logger.LogDebug($"{nameof(_serverTimeOffset)}: {_serverTimeOffset}");
                _logger.LogDebug($"{nameof(FakeDiscord._resetFakeOffset)}: {FakeDiscord._resetFakeOffset}");
                _logger.LogDebug($"{nameof(limit)}: {limit}");
                _logger.LogDebug($"{nameof(remaining)}: {remaining}");

                remaining--;

                context.Response.Headers.Clear();
                context.Response.Headers["Date"] = GetServerTime().ToString("r");
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = (reset.ToUnixTimestampSeconds() + FakeDiscord._resetFakeOffset).ToString();

                if (remaining < 0)
                {
                    FakeDiscord.RateLimitExceededCount++;
                    context.Response.StatusCode = 429;
                    await context.Response
                        .WriteAsync("{\"message\":\"You are being rate limited.\",\"retry_after\": "
                            + Math.Max(0, (int)(reset - GetServerTime()).TotalMilliseconds) + ",\"global\": false}");
                }
                else
                {
                    await context.Response
                        .WriteAsync("{\"id\": \"162701077035089920\",\"channel_id\": \"131391742183342080\",\"author\": {},\"content\": \"Hey guys!\",\"timestamp\": \"2016-03-24T23:15:59.605000+00:00\",\"edited_timestamp\": null,\"tts\": false,\"mention_everyone\": false,\"mentions\": [],\"mention_roles\": [],\"attachments\": [],\"embeds\": [],\"reactions\": []}");
                }
            });
        }

        DateTime GetServerTime()
        {
            return DateTime.UtcNow + _serverTimeOffset;
        }
    }
}