using System;
using System.Net.Http;
using Newtonsoft.Json;
using DiscordApiWrapper.RestApi.Extensions;
using System.Threading.Tasks;

namespace DiscordApiWrapper.RestApi
{
    public class RateLimitExceeded
    {
        [JsonIgnore]
        public RateLimit RateLimit;

        [JsonIgnore]
        public string Reason;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("retry_after")]
        int _retryAfterInMilliseconds;
        public TimeSpan RetryAfter
        {
            get {return TimeSpan.FromMilliseconds(_retryAfterInMilliseconds);}
            set {_retryAfterInMilliseconds = (int)value.TotalMilliseconds;}
        }

        [JsonProperty("global")]
        public bool Global;

        public static async Task<RateLimitExceeded> Create(HttpResponseMessage response)
        {
            var rateLimitExceeded = await response.DeserializeResponse<RateLimitExceeded>();
            rateLimitExceeded.RateLimit = response.GetRateLimit();
            rateLimitExceeded.Reason = response.ReasonPhrase;
            return rateLimitExceeded;
        }
    }
}