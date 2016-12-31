using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models.Events
{
    public class TypingStart
	{
		[JsonProperty("channel_id")]
		public ulong ChannelId;
		
		[JsonProperty("user_id")]
		public ulong UserId;
		
		[JsonProperty("timestamp")]
		public DateTime Timestamp;
	}
}
