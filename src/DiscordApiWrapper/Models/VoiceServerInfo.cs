using Newtonsoft.Json;

namespace DiscordApiWrapper.Models
{
    /// <summary>NEVER cache this info??or do?</summary>
    public class VoiceServerInfo
    {
        [JsonProperty("token")]
        public string Token;

        [JsonProperty("guild_id")]
        public ulong GuildID;

        [JsonProperty("endpoint")]
        public string Endpoint;
    }
}