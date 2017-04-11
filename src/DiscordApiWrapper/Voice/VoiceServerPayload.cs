using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
	class VoiceServerPayload {
		[JsonProperty("op")]
		public VoiceOpCode VoiceOpCode;
		
		[JsonProperty("d")]
		public string EventData;

		public VoiceServerPayload(VoiceOpCode voiceOpCode, string eventData) {
			VoiceOpCode = voiceOpCode;
			EventData = eventData;
		}
	}
}
