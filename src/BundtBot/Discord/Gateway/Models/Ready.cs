using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models {
	public class Ready {
		/// <summary>gateway protocol version</summary>
		[JsonProperty("v")]
		public int GatewayProtocolVersion;

		/// <summary>user object (with email information)</summary>
		[JsonProperty("user")]
		public object User;

		/// <summary>array of DM channel objects</summary>
		[JsonProperty("private_channels")]
		public object[] PrivateChannels;

		/// <summary>array of Unavailable Guild objects</summary>
		[JsonProperty("guilds")]
		public object[] UnavailableGuilds;

		/// <summary>used for resuming connections</summary>
		[JsonProperty("session_id")]
		public string SessionId;

		/// <summary>list of friends' presences (not applicable to bots)</summary>
		[JsonProperty("presences")]
		public object[] FriendsPresences;

		/// <summary>list of friends (not applicable to bots)</summary>
		[JsonProperty("relationships")]
		public object[] Friends;

		/// <summary>used for debugging, array of servers connected to</summary>
		[JsonProperty("_trace")]
		public string[] Trace;
	}
}
