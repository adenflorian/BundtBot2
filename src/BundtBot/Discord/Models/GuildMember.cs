using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class GuildMember
	{
		internal DiscordClient Client;

		[JsonProperty("user")]
		public User User;

		/// <summary>
		/// This users guild nickname (if one is set).
		/// </summary>
		[JsonProperty("nick")]
		public string Nickname;
		
		[JsonProperty("roles")]
		public object[] Roles;

		/// <summary>
		/// Date the user joined the guild.
		/// </summary>
		[JsonProperty("joined_at")]
		public DateTime JoinedAt;
		
		[JsonProperty("deaf")]
		public bool IsDeafened;
		
		[JsonProperty("mute")]
		public bool IsMuted;
	}
}
