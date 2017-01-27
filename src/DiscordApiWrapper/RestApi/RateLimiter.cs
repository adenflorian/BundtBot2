using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BundtBot.Discord
{
    public class RateLimiter
	{
        ConcurrentQueue<Guid> queue = new ConcurrentQueue<Guid>();
        int remainingRequests = 1;
        int _resetTime;

        public async Task DoFoo()
        {
            var myGuid = new Guid();
            queue.Enqueue(myGuid);

            while (true)
            {
                Guid result;
                if (queue.TryPeek(out result))
                {
                    if (result == myGuid)
                    {
                        break;
                    }
                }
            }

            if (remainingRequests > 0)
            {
                remainingRequests--;
                Guid result;
                queue.TryDequeue(out result);
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(_resetTime - getTimestamp()));

            {
                Guid result;
                queue.TryDequeue(out result);
            }
        }

        int getTimestamp()
        {
            return (int)Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public void Update(int remaining, int resetTime)
        {
            remainingRequests = remaining;
            _resetTime = resetTime;
        }
	}
}
