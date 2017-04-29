using DiscordApiWrapper.Gateway;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway
{
	class GatewayPayload {
		[JsonProperty("op")]
		public GatewayOpCode GatewayOpCode;
		
		[JsonProperty("d")]
		public object EventData;

		/// <summary>
		/// Used for resuming sessions and heartbeats, only used with OpCode 0.
		/// </summary>
		[JsonProperty("s")]
		public int? SequenceNumber;

        /// <summary>
        /// Only used with OpCode 0. GATEWAY_EVENT_NAME.
        /// </summary>
        [JsonProperty("t")]
        public GatewayEvent? EventName;

		public GatewayPayload(GatewayOpCode gatewayOpCode, object eventData) {
			GatewayOpCode = gatewayOpCode;
			EventData = eventData;
		}
	}
}
