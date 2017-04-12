using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
    public class Speaking
    {
        [JsonProperty("user_id")]
        public ulong UserId;

        /// <summary>https://tools.ietf.org/html/rfc3550#section-8</summary>
        [JsonProperty("ssrc")]
        public uint SyncSourceId;

        [JsonProperty("speaking")]
        public bool IsSpeaking;
    }
}