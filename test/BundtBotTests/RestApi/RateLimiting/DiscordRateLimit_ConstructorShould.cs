using Xunit;
using DiscordApiWrapper.RestApi;
using System;

namespace DiscordApiWrapperTests.DiscordRateLimit
{
    public class DiscordRateLimit_ConstructorShould
    {
        [Fact]
        void SetLimit()
        {
            var discordRateLimit = new DiscordApiWrapper.RestApi
                .DiscordRateLimit(7, 0, DateTime.MinValue, DateTime.MinValue, () => DateTime.MinValue);

            Assert.Equal(7, discordRateLimit.Limit);
        }

        [Fact]
        void SetRemaining()
        {
            var discordRateLimit = new DiscordApiWrapper.RestApi
                .DiscordRateLimit(0, 3, DateTime.MinValue, DateTime.MinValue, () => DateTime.MinValue);

            Assert.Equal(3, discordRateLimit.Remaining);
        }

        [Fact]
        void SetResetTimeToCurrentUtcTimePlusMaxTimeUntilReset()
        {
            Func<DateTime> currentUtcDateTimeProvider = () => DateTime.Parse("2017-02-06T04:54:30.4250000");
            var apiServerTime = DateTime.Parse("2017-02-06T04:54:31.4250000");
            var apiServerResetTime = DateTime.Parse("2017-02-06T04:54:35.4250000");

            var discordRateLimit = new DiscordApiWrapper.RestApi
                .DiscordRateLimit(0, 0, apiServerResetTime, apiServerTime, currentUtcDateTimeProvider);

            Assert.Equal(DateTime.Parse("2017-02-06T04:54:35.4250000"), discordRateLimit.ResetTime);
        }
    }
}