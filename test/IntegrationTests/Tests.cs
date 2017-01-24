using System.Threading;
using System.Threading.Tasks;
using FakeDiscordSharp;
using Xunit;

namespace Tests
{
    public class Tests
    {
        public Tests()
        {
            var fakeDiscord = new FakeDiscord();
            Task.Run(() => fakeDiscord.Start());
            Thread.Sleep(15000);
        }

        public void Dispose()
        {

        }

        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }
}