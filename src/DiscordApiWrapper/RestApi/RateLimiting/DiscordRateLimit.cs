using System;
using BundtBot;

namespace DiscordApiWrapper.RestApi
{
    public class DiscordRateLimit
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DiscordRateLimit));
        // X-RateLimit-Limit
        public int Limit { get; }

        // X-RateLimit-Remaining
        public int Remaining;

        // X-RateLimit-Reset
        // Header value is a unix timestamp in seconds
        public DateTime ResetTime;

        public DiscordRateLimit(int limit, int remaining, DateTime apiServerResetTime, DateTime apiServerTime,
            Func<DateTime> currentUtcDateTimeProvider)
        {
            var currentUtcDateTime = currentUtcDateTimeProvider.Invoke();

            Limit = limit;
            Remaining = remaining;

            var maxTimeSpanUntilReset = (apiServerResetTime - apiServerTime) + TimeSpan.FromSeconds(1);
            ResetTime = currentUtcDateTime + maxTimeSpanUntilReset;

            LogTraceLogs(apiServerResetTime, apiServerTime, currentUtcDateTime, maxTimeSpanUntilReset);
        }

        void LogTraceLogs(DateTime apiServerResetTime, DateTime apiServerTime,
            DateTime currentUtcDateTime, TimeSpan maxTimeSpanUntilReset)
        {
            _logger.LogTrace($"Discord Api Server Time: {apiServerTime.ToString("hh:mm:ss.fff")}");
            _logger.LogTrace($"Discord Api Server Reset Time: {apiServerResetTime.ToString("hh:mm:ss.fff")}");
            _logger.LogTrace($"Current UTC Time: {currentUtcDateTime.ToString("hh:mm:ss.fff")}");
            _logger.LogTrace($"Max time until reset: {maxTimeSpanUntilReset.TotalSeconds} seconds");
            _logger.LogTrace($"Final reset time: {ResetTime.ToString("hh:mm:ss.fff")}");
        }
    }
}