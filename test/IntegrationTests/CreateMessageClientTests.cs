using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using DiscordApiWrapper.RestApi;
using FakeDiscordSharp;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CreateMessageClientTests
    {
        ITestOutputHelper _output;

        public CreateMessageClientTests(ITestOutputHelper output)
        {
            _output = output;
            var fakeDiscord = new FakeDiscord();
            Task.Run(() => fakeDiscord.Start());
        }

        public void Dispose()
        {

        }

        [Fact]
        public void Test1()
        {
            return;
            int[] myArr = new int[99];
            List<Task> tasks = new List<Task>();

            int x = 1;
            
            tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));
            // tasks.Add(StartThread(x++, myArr));

            log($"Now waiting for {tasks.Count} to complete");

            Task.WhenAll(tasks).Wait();

            log($"{tasks.Count} tasks complete!");

            for (int i = 0; i < tasks.Count; i++)
            {
                log($"myArr[{i}]: {myArr[i]}");
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                Assert.True(myArr[i] == i + 1);
            }
        }

        Task StartThread(int i, int[] myArr)
        {
            return Task.Run(async () =>
            {
                log(i + " Start!");

                var apiUri = new Uri("http://localhost:5000/");
                var restClient = new DiscordRestClient("token", "name", "version", apiUri);
                var client = new CreateMessageClient(restClient);

                await client.CreateAsync((ulong)i, new CreateMessage{Content = "hello world " + i});

                myArr[i - 1] = i;
                log(i + " Done!");
            });
        }

        void log(string msg)
        {
            System.Console.WriteLine(msg);
            _output?.WriteLine(msg);
        }
    }
}