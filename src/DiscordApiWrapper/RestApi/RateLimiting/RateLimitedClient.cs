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
        static readonly MyLogger _logger = new MyLogger(nameof(RateLimitedClient), ConsoleColor.Magenta);
        public static TimeSpan _waitTimeCushionStart = TimeSpan.FromSeconds(2.5f);
        public static TimeSpan _waitTimeCushionIncrement = TimeSpan.FromSeconds(1);

        readonly ConcurrentQueue<Tuple<RestApiRequest, Action<HttpResponseMessage>>> _queue =
            new ConcurrentQueue<Tuple<RestApiRequest, Action<HttpResponseMessage>>>();
        readonly IRestRequestProcessor _innerProcessor;

        //bool _isGlobalRateLimitInEffect = false;
        //RateLimit _globalRateLimit;
        // Will be overriden each response
        RateLimit _rateLimit = new RateLimit(1, 1, UnixTime.GetTimestamp());
        TimeSpan _waitTimeCushion = _waitTimeCushionStart;

        public RateLimitedClient(IRestRequestProcessor innerProcessor)
        {
            _innerProcessor = innerProcessor;

            StartProcessRequestLoop();
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(RestApiRequest request)
        {
            HttpResponseMessage response = null;
            var notDone = true;

            _queue.Enqueue(Tuple.Create<RestApiRequest, Action<HttpResponseMessage>>(request, (msg) =>
            {
                response = msg;
                notDone = false;
            }));

            _logger.LogDebug($"Enqueued request {request.RequestType} {request.RequestUri}");

            while (notDone) await Task.Delay(100);

            return response;
        }

        void StartProcessRequestLoop()
        {
            Task.Run(async () => await ProcessRequestLoopAsync());
        }

        async Task ProcessRequestLoopAsync()
        {
            while (true)
            {
                await TryToProcessNextRequestAsync();
            }
        }

        async Task TryToProcessNextRequestAsync()
        {
            Tuple<RestApiRequest, Action<HttpResponseMessage>> result;

            if (_queue.TryDequeue(out result))
            {
                _logger.LogDebug($"Dequeued request {result.Item1.RequestType} {result.Item1.RequestUri}");
                await ProcessRequestAsync(result.Item1, result.Item2);
            }
            else
            {
                await Task.Delay(100);
            }
        }

        async Task ProcessRequestAsync(RestApiRequest request, Action<HttpResponseMessage> requestCompletedCallback)
        {
            await DecrementRemainingRequestsOrWaitForReset();

            try
            {
                await RequestAsync(request, requestCompletedCallback);
            }
            catch (RateLimitExceededException ex)
            {
                await OnRateLimitExceededAsync(ex);
                await RetryRequest(request, requestCompletedCallback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                _logger.LogError("Invoking callback with null");
                requestCompletedCallback.Invoke(null);
                return;
            }
        }

        async Task DecrementRemainingRequestsOrWaitForReset()
        {
            if (_rateLimit.Remaining == 0)
            {
                _logger.LogInfo($"Out of requests", ConsoleColor.Magenta);
                await DelayUntilReset();
            }
            else
            {
                _logger.LogDebug($"{_rateLimit.Remaining} request(s) available, using one...");
                _rateLimit.Remaining--;
            }
        }

        async Task DelayUntilReset()
        {
            var currentTime = UnixTime.GetTimestamp();
            _logger.LogDebug($"WaitUntilReset: currentTime: {currentTime} resetTime: {_rateLimit.Reset}", ConsoleColor.Magenta);

            var waitAmount = TimeSpan.FromSeconds(Math.Max(0, (_rateLimit.Reset - currentTime)));

            _logger.LogInfo($"WaitUntilReset: Waiting for {waitAmount.TotalSeconds} + {_waitTimeCushion.TotalSeconds}(cushion) seconds", ConsoleColor.Magenta);
            var finalWaitAmount = waitAmount + _waitTimeCushion;
            await Task.Delay(finalWaitAmount);
            _logger.LogInfo($"WaitUntilReset: Done waiting for {finalWaitAmount.TotalSeconds} seconds", ConsoleColor.Magenta);
        }

        async Task RequestAsync(RestApiRequest request, Action<HttpResponseMessage> requestCompletedCallback)
        {
            var response = await _innerProcessor.ProcessRequestAsync(request);
            UpdateRateLimitFrom(response);
            requestCompletedCallback.Invoke(response);
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

        async Task RetryRequest(RestApiRequest request, Action<HttpResponseMessage> requestCompletedCallback)
        {
            _logger.LogError("Retrying request...");
            try
            {
                await RequestAsync(request, requestCompletedCallback);
            }
            catch (Exception)
            {
                _logger.LogError("Retry failed");
                throw;
            }
        }

        void UpdateRateLimitFrom(HttpResponseMessage response)
        {
            if (response.Headers.Contains("X-RateLimit-Limit"))
            {
                _rateLimit = response.GetRateLimit();
            }
            else
            {
                _rateLimit = new RateLimit(999, 999, 0);
            }
        }
    }
}