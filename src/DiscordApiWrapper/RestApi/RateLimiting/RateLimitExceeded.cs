using System;
using System.Net.Http;
using Newtonsoft.Json;
using DiscordApiWrapper.RestApi.Extensions;

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

        public RateLimitExceeded(HttpResponseMessage response)
        {
            var rateLimitExceeded = response.DeserializeResponse<RateLimitExceeded>().GetAwaiter().GetResult();
            Message = rateLimitExceeded.Message;
            RetryAfter = rateLimitExceeded.RetryAfter;
            Global = rateLimitExceeded.Global;
            RateLimit = response.GetRateLimit();
            Reason = response.ReasonPhrase;
        }
    }
}