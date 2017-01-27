using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using BundtCommon;

namespace DiscordApiWrapper.RestApi
{
    public class CreateMessageClient
    {
        ConcurrentQueue<Tuple<ulong, CreateMessage, Action<DiscordMessage>>> _queue = new ConcurrentQueue<Tuple<ulong, CreateMessage, Action<DiscordMessage>>>();
        DiscordRestClient _restClient;
        RateLimit rateLimit = new RateLimit(1, 1, 1);

        public CreateMessageClient(DiscordRestClient restClient)
        {
            _restClient = restClient;

            Task.Run(async () => {
                while (true)
                {
                    Tuple<ulong, CreateMessage, Action<DiscordMessage>> result;
                    if (_queue.TryDequeue(out result))
                    {
                        Debug.Assert(rateLimit.Remaining >= 0);
                        if (rateLimit.Remaining == 0)
                        {
                            WaitUntilReset();
                        }
                        else
                        {
                            rateLimit.Remaining--;
                        }
                        var response = await _restClient.CreateMessageAsync(result.Item1, result.Item2);
                        rateLimit = response.Item2;
                        result.Item3.Invoke(response.Item1);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });
        }

        async void WaitUntilReset()
        {
            var waitAmount = TimeSpan.FromSeconds(Math.Max(0, rateLimit.Reset - UnixTime.GetTimestamp()));
            await Task.Delay(waitAmount);
        }

        public async Task<DiscordMessage> CreateAsync(ulong channelId, CreateMessage createMessage)
        {
            DiscordMessage message = null;

            _queue.Enqueue(Tuple.Create<ulong, CreateMessage, Action<DiscordMessage>>(channelId, createMessage, (msg) =>
            {
                message = msg;
            }));

            while (message == null)
            {
                await Task.Delay(100);
            }

            return message;
        }
    }
}