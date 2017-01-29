using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using DiscordApiWrapper.RestApi;
using FakeDiscordSharp;

class RateLimitTester
{
    static readonly MyLogger _logger = new MyLogger(nameof(RateLimitTester));
    static CreateMessageClient _msgClient;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var fakeDiscord = new FakeDiscord();
        Task.Run(() => fakeDiscord.Start());
        Task.Run(async () => {
            await Task.Delay(1);
        });

        var apiUri = new Uri("http://localhost:5000/");
        var restClient = new DiscordRestClient(new RestClientConfig("token", "name", "version", apiUri));
        _msgClient = new CreateMessageClient(restClient);

        Test1();
        _logger.LogInfo("Test1 Complete");
    }

    public static void Test1()
    {
        var queue = new ConcurrentQueue<int>();
        var tasks = new List<Task>();

        for (int i = 0; i < 11; i++)
        {
            tasks.Add(StartThread(i + 1, queue));
            Thread.Sleep(100);
        }

        log($"Now waiting for {tasks.Count} to complete");

        Task.WhenAll(tasks).Wait();

        log($"{tasks.Count} tasks complete!");

        var myArr = new int[queue.Count];

        for (int i = 0; i < tasks.Count; i++)
        {
            int result;
            queue.TryDequeue(out result);
            myArr[i] = result;
            log($"myArr[{i}]: {myArr[i]}");
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            if(myArr[i] != i + 1)
            {
                throw new Exception((i + 1).ToString());
            }
        }
    }

    static Task StartThread(int i, ConcurrentQueue<int> queue)
    {
        return Task.Run(async () =>
        {
            log(i + " Start!");

            var message = await _msgClient.CreateAsync((ulong)i, new CreateMessage{Content = "hello world " + i});
            
            if(message == null)
            {
                log(i + " null :(");
                return;
            }

            queue.Enqueue((int)i);
            log(i + " Done!");
        });
    }

    static void log(string msg)
    {
        _logger.LogInfo(msg);
    }
}
