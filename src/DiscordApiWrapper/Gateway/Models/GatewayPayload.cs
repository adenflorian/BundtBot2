using DiscordApiWrapper.Gateway;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway
{
    [JsonObject]
	public class GatewayPayload {
		[JsonRequired]
		[JsonProperty("op")]
		public OpCode GatewayOpCode;
		
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
		public string EventName;

		public GatewayPayload(OpCode gatewayOpCode, object eventData) {
			GatewayOpCode = gatewayOpCode;
			EventData = eventData;
		}

		public string Serialize() {
			var jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
			var jsonGatewayDispatch = JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
			return jsonGatewayDispatch;
		}

		public override string ToString() {
			return $"{nameof(GatewayOpCode)}: {GatewayOpCode}," +
			       $"{nameof(EventData)}: {EventData}," +
			       $"{nameof(SequenceNumber)}:" + $"{SequenceNumber}," +
			       $"{nameof(EventName)}: {EventName}";
		}
	}
}
