using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway {
	[JsonObject]
    public class GatewayResume {
        [JsonRequired]
        [JsonProperty("token")]
        public string SessionToken;

        [JsonRequired]
        [JsonProperty("session_id")]
        public string SessionId;

        [JsonRequired]
        [JsonProperty("seq")]
        public int LastSequenceNumberReceived;
	}
}
