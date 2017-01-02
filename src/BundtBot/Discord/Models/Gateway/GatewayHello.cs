using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Gateway
{
	[JsonObject]
	public class GatewayHello
	{
		/// <summary>
		/// The interval (in milliseconds) the client should heartbeat with.
		/// </summary>
		[JsonRequired]
		[JsonProperty("heartbeat_interval")]
		int _heartbeatInterval;
		public TimeSpan HeartbeatInterval {
			get { return TimeSpan.FromMilliseconds(_heartbeatInterval); }
			set { _heartbeatInterval = value.Milliseconds; }
		}

		/// <summary>
		/// Used for debugging. Array of servers connected to.
		/// </summary>
		[JsonProperty("_trace")]
		public string[] Trace;
	}
}
