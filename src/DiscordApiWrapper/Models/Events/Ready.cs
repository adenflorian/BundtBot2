using System.Collections.Generic;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models {
	public class Ready {
		[JsonProperty("v")]
		public int GatewayProtocolVersion;

		/// <summary>User object (with email information).</summary>
		[JsonProperty("user")]
		public DiscordUser User;

		/// <summary>Array of DM channel objects.</summary>
		[JsonProperty("private_channels")]
		public List<object> PrivateChannels;

		/// <summary>Array of Unavailable Guild objects.</summary>
		[JsonProperty("guilds")]
		public List<object> UnavailableGuilds;

		/// <summary>Used for resuming connections.</summary>
		[JsonProperty("session_id")]
		public string SessionId;

		/// <summary>List of friends' presences (not applicable to bots).</summary>
		[JsonProperty("presences")]
		public List<object> FriendsPresences;

		/// <summary>List of friends (not applicable to bots).</summary>
		[JsonProperty("relationships")]
		public List<object> Friends;

		/// <summary>Used for debugging, array of servers connected to.</summary>
		[JsonProperty("_trace")]
		public List<string> Trace;
	}
}
