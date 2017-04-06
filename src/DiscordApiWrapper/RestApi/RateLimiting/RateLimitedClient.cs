using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using DiscordApiWrapper.RestApi.Extensions;
using DiscordApiWrapper.RestApi.RestApiRequests;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi
{
    class RateLimitedClient : IRestRequestProcessor
    {
        public static readonly TimeSpan _waitTimeCushionIncrement = TimeSpan.FromSeconds(0.5f);

        static readonly MyLogger _logger = new MyLogger(nameof(RateLimitedClient), ConsoleColor.Magenta);

        readonly ConcurrentQueue<Tuple<RestApiRequest, Action<HttpResponseMessage>>> _queue =
            new ConcurrentQueue<Tuple<RestApiRequest, Action<HttpResponseMessage>>>();
        readonly IRestRequestProcessor _innerProcessor;

        int _limit = 1;
        int _remainingAllowedRequests = 1;
        DateTime _resetTimeUtc;
        TimeSpan _waitTimeCushion;

        public RateLimitedClient(IRestRequestProcessor innerProcessor) : this(innerProcessor, TimeSpan.Zero) { }

        public RateLimitedClient(IRestRequestProcessor innerProcessor, TimeSpan waitTimeCushionStart)
        {
            _innerProcessor = innerProcessor;
            _waitTimeCushion = waitTimeCushionStart;

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
                // TODO This is BAD!
                requestCompletedCallback.Invoke(null);
                return;
            }
        }

        async Task DecrementRemainingRequestsOrWaitForReset()
        {
            if (_remainingAllowedRequests == 0)
            {
                _logger.LogInfo($"Out of requests", ConsoleColor.Magenta);
                await DelayUntilReset();
                OnReset();
            }
            else
            {
                _logger.LogDebug($"{_remainingAllowedRequests} request(s) available, using one...");
                _remainingAllowedRequests--;
            }
        }

        async Task DelayUntilReset()
        {
            var currentTimeUtc = DateTime.UtcNow;
            _logger.LogDebug($"WaitUntilReset: currentTime: {currentTimeUtc.ToString("hh:mm:ss.fff")} resetTime: {_resetTimeUtc.ToString("hh:mm:ss.fff")}", ConsoleColor.Magenta);

            var timeDiff = _resetTimeUtc - currentTimeUtc;
            var waitAmount = timeDiff.TotalMilliseconds >= 0 ? timeDiff : TimeSpan.Zero;

            _logger.LogInfo($"WaitUntilReset: Waiting for {waitAmount.TotalSeconds} + {_waitTimeCushion.TotalSeconds}(cushion) seconds", ConsoleColor.Magenta);
            var finalWaitAmount = waitAmount + _waitTimeCushion;
            await Task.Delay(finalWaitAmount);
            _logger.LogInfo($"WaitUntilReset: Done waiting for {finalWaitAmount.TotalSeconds} seconds", ConsoleColor.Magenta);
        }

        void OnReset()
        {
            _remainingAllowedRequests = _limit;
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
            UpdateRateLimitFrom(ex.RateLimitExceeded);
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

        void UpdateRateLimitFrom(RateLimitExceeded rateLimitExceeded)
        {
            UpdateRateLimitFrom(rateLimitExceeded.RateLimit);
        }

        void UpdateRateLimitFrom(HttpResponseMessage response)
        {
            if (response.Headers.Contains("X-RateLimit-Limit"))
            {
                UpdateRateLimitFrom(response.GetRateLimit());
            }
            else
            {
                _logger.LogWarning("Response does not have X-RateLimit-Limit header");
                _remainingAllowedRequests++;
            }
        }

        void UpdateRateLimitFrom(DiscordRateLimit discordRateLimit)
        {
            _remainingAllowedRequests = discordRateLimit.Remaining;
            _limit = discordRateLimit.Limit;
            _resetTimeUtc = discordRateLimit.ResetTime;
        }
    }
}