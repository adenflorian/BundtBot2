using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FakeDiscordSharp
{
    public class FakeDiscord
    {
        public void Start()
        {
            Console.WriteLine("Running demo with Kestrel.");

            var config = new ConfigurationBuilder()
                .Build();

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
                .UseUrls("http://localhost:5000");

            var host = builder.Build();
            host.Run();
        }
    }
}
