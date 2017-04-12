using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice.VoiceGateway
{
    public class VoiceSelectProtocolData
    {
        [JsonProperty("address")]
        public string ClientPublicIpAddress;

        [JsonProperty("port")]
        public int ClientPort;

        [JsonProperty("mode")]
        public string EncryptionMethod;
    }
}