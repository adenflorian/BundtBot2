using System;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord.Gateway;
using FakeDiscordSharp;
using Microsoft.Extensions.Logging;
using Xunit;

public class GatewayClientTester
{
    static readonly MyLogger _logger = new MyLogger(nameof(GatewayClientTester));

    [Fact]
    public async Task TestAsync()
    {
        _logger.SetLogLevel(LogLevel.Trace);
        DiscordGatewayClient client = new DiscordGatewayClient("authtoken", new Uri("ws://localhost:8001"));

        await client.ConnectAsync();

        await Task.Delay(5000);

        Assert.Equal(0, FakeDiscord.RateLimitExceededCount);
    }
}
