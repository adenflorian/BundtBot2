using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice.VoiceGateway
{
    public class VoiceSelectProtocol
    {
        [JsonProperty("protocol")]
        public string Protocol;

        [JsonProperty("data")]
        public VoiceSelectProtocolData Data;
    }
}