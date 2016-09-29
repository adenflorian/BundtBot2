using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models {
	[JsonObject]
	public class ConnectionProperties {
		[JsonRequired]
		[JsonProperty("$os")]
		public string OperatingSystem;

		[JsonRequired]
		[JsonProperty("$browser")]
		public string Browser;

		[JsonRequired]
		[JsonProperty("$device")]
		public string Device;

		[JsonProperty("$referrer")]
		public string Referrer;

		[JsonProperty("$referring_domain")]
		public string ReferringDomain;
	}
}
