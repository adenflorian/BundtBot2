using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice.VoiceGateway
{
    public class VoiceServerSession
    {
        [JsonProperty("secret_key")]
        public byte[] SecretKey;

        [JsonProperty("mode")]
        public string Mode;
    }
}