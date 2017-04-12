using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
	class VoiceServerPayload {
		[JsonProperty("op")]
		public VoiceOpCode VoiceOpCode;
		
		[JsonProperty("d")]
		public object EventData;

		public VoiceServerPayload(VoiceOpCode voiceOpCode, object eventData) {
			VoiceOpCode = voiceOpCode;
			EventData = eventData;
		}
	}
}
