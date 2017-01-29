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
            //Thread.Sleep(10000);
            var fakeDiscord = new FakeDiscord();
            Task.Run(() => fakeDiscord.Start());
            Task.Run(async () => {
                await Task.Delay(1);
                //Assert.True(1 == 2);
            });
        }

        public void Dispose()
        {

        }

        [Fact]
        public void Test1()
        {
            int[] myArr = new int[99];
            List<Task> tasks = new List<Task>();

            int x = 1;
            
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));
            tasks.Add(StartThread(x++, myArr));

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
                var restClient = new DiscordRestClient(new RestClientConfig("token", "name", "version", apiUri));
                var client = new CreateMessageClient(restClient);

                var message = await client.CreateAsync((ulong)i, new CreateMessage{Content = "hello world " + i});
                
                log(i + " received message?!");
                Assert.NotNull(message);

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