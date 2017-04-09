using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
    public class VoiceServerIdentify
    {
        [JsonProperty("server_id")]
        public ulong GuildId;

        [JsonProperty("token")]
        public string VoiceServerToken;

        [JsonProperty("user_id")]
        public ulong UserId;

        [JsonProperty("session_id")]
        public string SessionId;
    }
}