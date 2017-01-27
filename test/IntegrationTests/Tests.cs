using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BundtBot.Discord;
using FakeDiscordSharp;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class Tests
    {
        ITestOutputHelper _output;

        public Tests(ITestOutputHelper output)
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

            _output.WriteLine($"Now waiting for {tasks.Count} to complete");

            Task.WhenAll(tasks).Wait();

            _output.WriteLine($"{tasks.Count} tasks complete!");

            for (int i = 0; i < tasks.Count; i++)
            {
                _output.WriteLine($"myArr[{i}]: {myArr[i]}");
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                Assert.True(myArr[i] == i + 1);
            }
        }

        Task StartThread(int i, int[] myArr)
        {
            return Task.Run(async () => {
                var rateLimiter = new RateLimiter();
                _output.WriteLine(i + " Start!");
                await rateLimiter.DoFoo();

                var client = new HttpClient();

                await client.GetAsync("http://localhost:5000");

                myArr[i - 1] = i;
                _output.WriteLine(i + " Done!");
            });
        }
    }
}