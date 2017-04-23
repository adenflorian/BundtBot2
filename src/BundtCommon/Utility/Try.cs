using System;
using System.Threading.Tasks;

namespace BundtCommon
{
    public static class Try
    {
        public static async Task Async(Func<Task<bool>> thingToTry, int times, TimeSpan interval)
        {
            int timesTried = 0;
            while (timesTried < times)
            {
                if (await thingToTry.Invoke()) return;
                timesTried++;
                await Task.Delay(interval);
            }
        }
        
        public static async Task ForeverAsync(Func<Task<bool>> thingToTry, TimeSpan interval)
        {
            while (true)
            {
                if (await thingToTry.Invoke()) return;
                await Task.Delay(interval);
            }
        }
    }
}