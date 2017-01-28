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
        public int retryAfter;

        [JsonProperty("global")]
        public bool global;
    }
}