using System;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi
{
    public class RateLimitExceeded
    {
        public RateLimit RateLimit;

        public string reason;

        [JsonProperty("message")]
        public string message;

        [JsonProperty("retry_after")]
        int _retryAfterInMilliseconds;
        public TimeSpan RetryAfter
        {
            get {return TimeSpan.FromMilliseconds(_retryAfterInMilliseconds);}
            set {_retryAfterInMilliseconds = (int)value.TotalMilliseconds;}
        }

        [JsonProperty("global")]
        public bool global;
    }
}