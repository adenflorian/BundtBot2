using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using BundtCommon;

namespace DiscordApiWrapper.RestApi
{
    public class CreateMessageClient
    {
		static readonly MyLogger _logger = new MyLogger(nameof(CreateMessageClient));
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
                        _logger.LogInfo($"Dequeued create message for channel {result.Item1}");
                        Debug.Assert(rateLimit.Remaining >= 0);
                        if (rateLimit.Remaining == 0)
                        {
                            _logger.LogInfo($"Out of requests");
                            await WaitUntilReset();
                        }
                        else
                        {
                            rateLimit.Remaining--;
                        }
                        try
                        {
                            var response = await _restClient.CreateMessageAsync(result.Item1, result.Item2);
                            rateLimit = response.Item2;
                            result.Item3.Invoke(response.Item1);
                            await Task.Delay(1000);
                        }
                        catch (System.Exception)
                        {
                            result.Item3.Invoke(null);
                            throw;
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });
        }

        async Task WaitUntilReset()
        {
            var waitAmount = TimeSpan.FromSeconds(Math.Max(0, (rateLimit.Reset - UnixTime.GetTimestamp() + 1)));
            _logger.LogInfo($"Waiting for {waitAmount.Seconds} seconds");
            await Task.Delay(waitAmount);
            _logger.LogInfo($"Done waiting for {waitAmount.Seconds} seconds");
        }

        public async Task<DiscordMessage> CreateAsync(ulong channelId, CreateMessage createMessage)
        {
            DiscordMessage message = null;
            var notDone = true;

            _queue.Enqueue(Tuple.Create<ulong, CreateMessage, Action<DiscordMessage>>(channelId, createMessage, (msg) =>
            {
                message = msg;
                notDone = false;
            }));

            _logger.LogInfo($"CreateAsync: Enqueued create message for channel {channelId}");

            while (notDone)
            {
                await Task.Delay(100);
            }

            return message;
        }
    }
}