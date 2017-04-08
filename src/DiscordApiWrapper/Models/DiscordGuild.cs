using System;
using System.Collections.Generic;
using DiscordApiWrapper.Models;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class DiscordGuild
	{
		[JsonProperty("id")]
		public ulong Id;

		/// <summary>
		/// 2-100 characters
		/// </summary>
		[JsonProperty("name")]
		public string Name;

		[JsonProperty("icon")]
		public string IconHash;

		[JsonProperty("splash")]
		public string SplashHash;

		[JsonProperty("owner_id")]
		public ulong OwnerId;

		[JsonProperty("region")]
		public string VoiceRegionId;

		[JsonProperty("afk_channel_id")]
		public ulong? AfkChannelId;

		[JsonProperty("afk_timeout")]
		int _afkTimeoutInSeconds;
		public TimeSpan AfkTimeout {
			get { return TimeSpan.FromSeconds(_afkTimeoutInSeconds); }
			set { _afkTimeoutInSeconds = (int)value.TotalSeconds; }
		}

		[JsonProperty("embed_enabled")]
		public bool IsGuildEmbeddable;

		[JsonProperty("embed_channel_id")]
		public ulong EmbeddedChannelId;

		[JsonProperty("verification_level")]
		public int VerificationLevel;

		[JsonProperty("default_message_notifications")]
		public int DefaultMessageNotificationsLevel;

		[JsonProperty("roles")]
		public List<object> Roles;

		[JsonProperty("emojis")]
		public List<Emoji> Emojis;

		[JsonProperty("features")]
		public List<object> Features;

		[JsonProperty("mfa_level")]
		public int MultiFactorAuthenticationLevel;

		/// <summary>
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("joined_at")]
		public DateTime JoinedDate;

		/// <summary>
		/// Whether this is considered a large guild.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("large")]
		public bool IsLarge;

		/// <summary>
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("unavailable")]
		public bool IsUnavailable;

		[JsonProperty("member_count")]
		public int MemberCount;

		/// <summary>
		/// Array of voice state objects (without the guild_id key).
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("voice_states")]
		public List<VoiceState> VoiceStates;

		/// <summary>
		/// Array of guild member objects.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("members")]
		public List<GuildMember> Members;

		/// <summary>
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("channels")]
		public List<GuildChannel> AllChannels;

		/// <summary>
		/// Array of simple presence objects,
		/// which share the same fields as Presence Update event sans a roles or guild_id key.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("presences")]
		public List<object> Presences;

		internal DiscordGuild() {}
	}
}
