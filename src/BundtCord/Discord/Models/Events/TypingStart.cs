using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BundtBot.Discord.Models.Events
{
    public class TypingStart
	{
		[JsonProperty("channel_id")]
		public ulong ChannelId;
		
		[JsonProperty("user_id")]
		public ulong UserId;
		
		[JsonProperty("timestamp")]
		[JsonConverter(typeof(MyDateTimeConverter))]
		public DateTime Timestamp;
	}
}
