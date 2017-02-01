using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FakeDiscordSharp
{
    public class FakeDiscord
    {
        public static int RateLimitExceededCount = 0;

        int _port;
        internal static int _resetFakeOffset;

        public FakeDiscord(int port = 5000, int resetFakeOffset = 0)
        {
            RateLimitExceededCount = 0;
            _port = port;
            _resetFakeOffset = resetFakeOffset;
        }

        public void Start()
        {
            Console.WriteLine("Running demo with Kestrel.");

            var configBuilder = new ConfigurationBuilder();
            //configBuilder.Properties.Add("_resetFakeOffset", _resetFakeOffset);
            //Action p = () => { RateLimitExceededCount++; };
            //configBuilder.Properties.Add("IncrementRateLimitExceededCount", p);
            var config = configBuilder.Build();

            var builder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    if (config["threadCount"] != null)
                    {
                        options.ThreadCount = int.Parse(config["threadCount"]);
                    }
                })
                .UseUrls($"http://localhost:{_port}");

            var host = builder.Build();
            host.Run();
        }
    }
}
