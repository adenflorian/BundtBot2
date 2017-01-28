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

        readonly ConcurrentQueue<Tuple<ulong, CreateMessage, Action<DiscordMessage>>> _queue = new ConcurrentQueue<Tuple<ulong, CreateMessage, Action<DiscordMessage>>>();
        readonly DiscordRestClient _restClient;

        // Will be overriden each response
        RateLimit _rateLimit = new RateLimit(1, 1, UnixTime.GetTimestamp());

        public CreateMessageClient(DiscordRestClient restClient)
        {
            _restClient = restClient;

            Task.Run(async () => await LoopAsync());
        }

        async Task LoopAsync()
        {
            while (true)
            {
                Tuple<ulong, CreateMessage, Action<DiscordMessage>> result;
                if (_queue.TryDequeue(out result))
                {
                    _logger.LogInfo($"Dequeued create message for channel {result.Item1}");
                    Debug.Assert(_rateLimit.Remaining >= 0);
                    if (_rateLimit.Remaining == 0)
                    {
                        _logger.LogInfo($"Out of requests", ConsoleColor.Yellow);
                        await WaitUntilReset();
                    }
                    else
                    {
                        _logger.LogInfo($"{_rateLimit.Remaining} request(s) available, using one...");
                        _rateLimit.Remaining--;
                    }
                    try
                    {
                        var response = await _restClient.CreateMessageAsync(result.Item1, result.Item2);
                        _rateLimit = response.Item2;
                        _logger.LogInfo($"{_rateLimit.Remaining} request(s) remaining");
                        result.Item3.Invoke(response.Item1);
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
        }

        async Task WaitUntilReset()
        {
            var currentTime = UnixTime.GetTimestamp();
            _logger.LogInfo($"WaitUntilReset: currentTime: {currentTime} resetTime: {_rateLimit.Reset}", ConsoleColor.Yellow);

            // Adding 2.5 seconds to wait time based off of real world testing
            var waitAmount = TimeSpan.FromSeconds(Math.Max(0, (_rateLimit.Reset - currentTime)) + 2.5f);

            _logger.LogInfo($"WaitUntilReset: Waiting for {waitAmount.Seconds} seconds", ConsoleColor.Yellow);
            await Task.Delay(waitAmount);
            _logger.LogInfo($"WaitUntilReset: Done waiting for {waitAmount.Seconds} seconds", ConsoleColor.Yellow);
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

            while (notDone) await Task.Delay(100);

            return message;
        }
    }
}