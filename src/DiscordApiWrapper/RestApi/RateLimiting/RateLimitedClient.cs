using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using BundtCommon;
using DiscordApiWrapper.RestApi.Extensions;
using DiscordApiWrapper.RestApi.RestApiRequests;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi
{
    class RateLimitedClient : IRestRequestProcessor
    {
        static readonly MyLogger _logger = new MyLogger(nameof(RateLimitedClient));
        static readonly TimeSpan _waitTimeCushionStart = TimeSpan.FromSeconds(2.5f);
        static readonly TimeSpan _waitTimeCushionIncrement = TimeSpan.FromSeconds(1);

        readonly ConcurrentQueue<Tuple<IRestApiRequest, Action<HttpResponseMessage>>> _queue =
            new ConcurrentQueue<Tuple<IRestApiRequest, Action<HttpResponseMessage>>>();
        readonly IRestRequestProcessor _innerProcessor;

        // Will be overriden each response
        RateLimit _rateLimit = new RateLimit(1, 1, UnixTime.GetTimestamp());
        TimeSpan _waitTimeCushion = _waitTimeCushionStart;

        public RateLimitedClient(IRestRequestProcessor innerProcessor)
        {
            _innerProcessor = innerProcessor;

            StartSendMessageLoop();
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(IRestApiRequest request)
        {
            HttpResponseMessage response = null;
            var notDone = true;

            _queue.Enqueue(Tuple.Create<IRestApiRequest, Action<HttpResponseMessage>>(request, (msg) => {
                response = msg;
                notDone = false;
            }));

            _logger.LogDebug($"Enqueued request {request.RequestType} {request.RequestUri}");

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
            Tuple<IRestApiRequest, Action<HttpResponseMessage>> result;

            if (_queue.TryDequeue(out result))
            {
                await SendMessageAsync(result.Item1, result.Item2);
            }
            else
            {
                await Task.Delay(100);
            }
        }

        async Task SendMessageAsync(IRestApiRequest request, Action<HttpResponseMessage> callback)
        {
            _logger.LogDebug($"Dequeued request {request.RequestType} {request.RequestUri}");

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

            HttpResponseMessage response = null;

            try
            {
                response = await _innerProcessor.ProcessRequestAsync(request);
            }
            catch (RateLimitExceededException ex)
            {
                try
                {
                    await OnRateLimitExceededAsync(ex);
                    _logger.LogError("Retrying request...");
                    response = await _innerProcessor.ProcessRequestAsync(request);
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

            if (response.Headers.Contains("X-RateLimit-Limit"))
            {
                _rateLimit = response.GetRateLimit();
            }
            else
            {
                _rateLimit = new RateLimit(999, 999, 0);
            }

            callback.Invoke(response);
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