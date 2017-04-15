using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice.VoiceGateway
{
    public class VoiceServerSpeakingServer
    {
        [JsonProperty("user_id")]
        public ulong UserId;

        [JsonProperty("speaking")]
        public bool IsSpeaking;

        [JsonProperty("ssrc")]
        public uint SynchronizationSourceId;
    }
}