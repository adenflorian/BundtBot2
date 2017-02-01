using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.RestApi.RestApiRequests;
using FakeDiscordSharp;
using Xunit;

public class RateLimitTester
{
    static readonly MyLogger _logger = new MyLogger(nameof(RateLimitTester));

    [Fact]
    public void ShouldNotExceedRateLimitWhenServerIsFair()
    {
        var port = 5000;
        var fakeDiscord = new FakeDiscord(port);
        Task.Run(() => fakeDiscord.Start());

        var apiUri = new Uri($"http://localhost:{port}/");
        var restClient = new DiscordRestClient(new RestClientConfig("token", "name", "version", apiUri));
        var _rateLimitedClient = new RateLimitedClient(restClient);

        var queue = new ConcurrentQueue<int>();
        var tasks = new List<Task>();

        for (int i = 0; i < 11; i++)
        {
            tasks.Add(_rateLimitedClient.ProcessRequestAsync(new NewMessageRequest((ulong)i) { Content = "hello world " + i }));
        }

        _logger.LogInfo($"Now waiting for {tasks.Count} to complete");

        Task.WhenAll(tasks).Wait();

        _logger.LogInfo($"{tasks.Count} tasks complete!");

        Assert.Equal(0, FakeDiscord.RateLimitExceededCount);
    }

    [Fact]
    public void ShouldExceedRateLimitOnceWhenServerIsOffByOneSecond()
    {
        var port = 5001;
        var fakeDiscord = new FakeDiscord(port, -1);
        Task.Run(() => fakeDiscord.Start());

        var apiUri = new Uri($"http://localhost:{port}/");
        var restClient = new DiscordRestClient(new RestClientConfig("token", "name", "version", apiUri));
        RateLimitedClient._waitTimeCushionStart = TimeSpan.FromSeconds(0);
        var _rateLimitedClient = new RateLimitedClient(restClient);

        var queue = new ConcurrentQueue<int>();
        var tasks = new List<Task>();

        for (int i = 0; i < 6; i++)
        {
            tasks.Add(_rateLimitedClient.ProcessRequestAsync(new NewMessageRequest((ulong)i) { Content = "hello world " + i }));
        }

        _logger.LogInfo($"Now waiting for {tasks.Count} to complete");

        Task.WhenAll(tasks).Wait();

        tasks.ForEach(x => x.GetAwaiter().GetResult());

        _logger.LogInfo($"{tasks.Count} tasks complete!");

        Assert.Equal(1, FakeDiscord.RateLimitExceededCount);
    }
}
