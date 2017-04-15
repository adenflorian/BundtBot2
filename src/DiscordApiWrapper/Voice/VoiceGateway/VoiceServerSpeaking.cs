using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice.VoiceGateway
{
    public class VoiceServerSpeakingClient
    {
        [JsonProperty("speaking")]
        public bool IsSpeaking;

        [JsonProperty("delay")]
        public uint Delay;
    }
}