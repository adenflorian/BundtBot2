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

        public DiscordRateLimit(int limit, int remaining, DateTime apiServerResetTime, DateTime apiServerTime)
        {
            _logger.LogTrace($"Discord Api Server Time: {apiServerTime.ToString("hh:mm:ss.fff")}");
            _logger.LogTrace($"Discord Api Server Reset Time: {apiServerResetTime.ToString("hh:mm:ss.fff")}");
            _logger.LogTrace($"Current UTC Time: {DateTime.UtcNow.ToString("hh:mm:ss.fff")}");

            Limit = limit;
            Remaining = remaining;

            var maxTimeUntilReset = (apiServerResetTime - apiServerTime) + TimeSpan.FromSeconds(1);
            ResetTime = DateTime.UtcNow + maxTimeUntilReset;

            _logger.LogTrace($"Max time until reset: {maxTimeUntilReset.TotalSeconds} seconds");
            _logger.LogTrace($"Final reset time: {ResetTime.ToString("hh:mm:ss.fff")}");
        }
    }
}