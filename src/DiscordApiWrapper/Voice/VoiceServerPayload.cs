using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
	class VoiceServerPayload {
		[JsonProperty("op")]
		public VoiceOpCode VoiceOpCode;
		
		[JsonProperty("d")]
		public object EventData;

        /// <summary>
        /// ?
        /// </summary>
        [JsonProperty("s")]
        public int? SequenceNumber;

		public VoiceServerPayload(VoiceOpCode voiceOpCode, object eventData) {
			VoiceOpCode = voiceOpCode;
			EventData = eventData;
		}

		public string Serialize() {
			var jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
			var jsonGatewayDispatch = JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
			return jsonGatewayDispatch;
		}
	}
}
