using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway {
	[JsonObject]
    public class GatewayIdentify {
		[JsonRequired]
		[JsonProperty("token")]
		public string AuthenticationToken;

		[JsonRequired]
		[JsonProperty("properties")]
		public ConnectionProperties ConnectionProperties;

		/// <summary>whether this connection supports compression of the initial ready packet</summary>
		[JsonProperty("compress")]
		public bool SupportsCompression;

		/// <summary>value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list</summary>
		[JsonProperty("large_threshold")]
		public Threshold LargeThreshold;

		/// <summary>Array of two integers(shard_id, num_shards)</summary>
		[JsonProperty("shard")]
		public int[] Shard;
	}

	public enum Threshold {
		Minimum = 50,
		Maximum = 250
	}
}
