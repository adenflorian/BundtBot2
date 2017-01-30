using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using BundtCommon;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi
{
    public class RateLimitedClient
    {
        static readonly MyLogger _logger = new MyLogger(nameof(RateLimitedClient));
        static readonly TimeSpan _waitTimeCushionStart = TimeSpan.FromSeconds(2.5f);
        static readonly TimeSpan _waitTimeCushionIncrement = TimeSpan.FromSeconds(1);

        readonly ConcurrentQueue<Tuple<IRestApiRequest, Action<string>>> _queue = new ConcurrentQueue<Tuple<IRestApiRequest, Action<string>>>();
        readonly DiscordRestClient _restClient;

        // Will be overriden each response
        RateLimit _rateLimit = new RateLimit(1, 1, UnixTime.GetTimestamp());
        TimeSpan _waitTimeCushion = _waitTimeCushionStart;

        public RateLimitedClient(DiscordRestClient restClient)
        {
            _restClient = restClient;

            StartSendMessageLoop();
        }

        public async Task<string> CreateAsync(IRestApiRequest request)
        {
            string response = null;
            var notDone = true;

            _queue.Enqueue(Tuple.Create<IRestApiRequest, Action<string>>(request, (msg) =>
            {
                response = msg;
                notDone = false;
            }));

            _logger.LogDebug($"Enqueued request {request.requestType} {request.requestUri}");

            while (notDone) await Task.Delay(100);

            return response;
        }

        void StartSendMessageLoop()
        {
            Task.Run(async () => await SendMessageLoopAsync());
        }

        async Task SendMessageLoopAsync()
        {
            while (true)
            {
                await TryToSendNextMessageAsync();
            }
        }

        async Task TryToSendNextMessageAsync()
        {
            Tuple<IRestApiRequest, Action<string>> result;

            if (_queue.TryDequeue(out result))
            {
                await SendMessageAsync(result.Item1, result.Item2);
            }
            else
            {
                await Task.Delay(100);
            }
        }

        async Task SendMessageAsync(IRestApiRequest request, Action<string> callback)
        {
            _logger.LogDebug($"Dequeued request {request.requestType} {request.requestUri}");

            if (_rateLimit.Remaining == 0)
            {
                _logger.LogInfo($"Out of requests", ConsoleColor.Magenta);
                await WaitUntilReset();
            }
            else
            {
                _logger.LogDebug($"{_rateLimit.Remaining} request(s) available, using one...");
                _rateLimit.Remaining--;
            }

            Tuple<string, RateLimit> response = null;

            try
            {
                response = await _restClient.PostRequestAsync(request);
            }
            catch (RateLimitExceededException ex)
            {
                await OnRateLimitExceededAsync(ex);
                try
                {
                    _logger.LogError("Retrying request...");
                    response = await _restClient.PostRequestAsync(request);
                }
                catch (System.Exception ex2)
                {
                    _logger.LogError("Retry failed");
                    _logger.LogError("Will invoke callback with null and carry on");
                    _logger.LogError(ex2);
                    callback.Invoke(null);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                _logger.LogError("Invoking callback with null");
                callback.Invoke(null);
                return;
            }

            _rateLimit = response.Item2;
            callback.Invoke(response.Item1);
        }

        async Task OnRateLimitExceededAsync(RateLimitExceededException ex)
        {
            _logger.LogError("Exceeded rate limit: " + JsonConvert.SerializeObject(ex.RateLimitExceeded, Formatting.Indented));
            _logger.LogError(ex);
            _rateLimit = ex.RateLimitExceeded.RateLimit;
            _logger.LogError($"OnRateLimitExceededAsync: Increasing wait cushion from {_waitTimeCushion.TotalSeconds} seconds "
                + $"to {(_waitTimeCushion + _waitTimeCushionIncrement).TotalSeconds} seconds");
            _waitTimeCushion += _waitTimeCushionIncrement;
            _logger.LogError($"OnRateLimitExceededAsync: Waiting for {ex.RateLimitExceeded.RetryAfter.TotalSeconds} + {_waitTimeCushion.TotalSeconds}(cushion) seconds");
            await Task.Delay(ex.RateLimitExceeded.RetryAfter + _waitTimeCushion);
        }

        async Task WaitUntilReset()
        {
            var currentTime = UnixTime.GetTimestamp();
            _logger.LogDebug($"WaitUntilReset: currentTime: {currentTime} resetTime: {_rateLimit.Reset}", ConsoleColor.Magenta);

            var waitAmount = TimeSpan.FromSeconds(Math.Max(0, (_rateLimit.Reset - currentTime)));

            _logger.LogInfo($"WaitUntilReset: Waiting for {waitAmount.TotalSeconds} + {_waitTimeCushion.TotalSeconds}(cushion) seconds", ConsoleColor.Magenta);
            var finalWaitAmount = waitAmount + _waitTimeCushion;
            await Task.Delay(finalWaitAmount);
            _logger.LogInfo($"WaitUntilReset: Done waiting for {finalWaitAmount.TotalSeconds} seconds", ConsoleColor.Magenta);
        }
    }
}