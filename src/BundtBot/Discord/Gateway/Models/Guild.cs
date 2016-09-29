using System;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models {
	public class Guild {
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
		int _afkTimeout;
		public TimeSpan AfkTimeout {
			get { return TimeSpan.FromSeconds(_afkTimeout); }
			set { _afkTimeout = value.Seconds; }
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
		public object[] Roles;

		[JsonProperty("emojis")]
		public object[] Emojis;

		[JsonProperty("features")]
		public object[] Features;

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
		public object[] VoiceStates;

		/// <summary>
		/// Array of guild member objects.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("members")]
		public object[] Members;

		/// <summary>
		/// Array of channel objects.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("channels")]
		public object[] Channels;

		/// <summary>
		/// Array of simple presence objects,
		/// which share the same fields as Presence Update event sans a roles or guild_id key.
		/// Only sent within the GUILD_CREATE event.
		/// </summary>
		[JsonProperty("presences")]
		public object[] Presences;
	}
}
