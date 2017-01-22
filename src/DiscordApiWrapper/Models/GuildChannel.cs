using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
	public enum GuildChannelType
	{
		Text,
		Voice
	}

    public class GuildChannel : Channel
    {
		[JsonProperty("guild_id")]
		public ulong GuildID;

		/// <summary>
		/// 2-100 characters.
		/// </summary>
		[JsonProperty("name")]
		public string Name;

		/// <summary>
		/// "text" or "voice".
		/// </summary>
		[JsonProperty("type")]
		public GuildChannelType Type;

		/// <summary>
		/// Sorting position of the channel.
		/// </summary>
		[JsonProperty("position")]
		public int Position;
		
		[JsonProperty("permission_overwrites")]
		public Overwrite[] PermissionOverwrites;

		/// <summary>
		/// 0-1024 characters.
		/// Present: Text only.
		/// </summary>
		[JsonProperty("topic")]
		public string Topic;

		/// <summary>
		/// Present: Text only.
		/// </summary>
		[JsonProperty("last_message_id")]
		public ulong? LastMessageId;

		/// <summary>
		/// The bitrate (in bits) of the voice channel.
		/// Present: Voice only.
		/// </summary>
		[JsonProperty("bitrate")]
		public int? Bitrate;

		/// <summary>
		/// the user limit of the voice channel.
		/// Present: Voice only.
		/// </summary>
		[JsonProperty("user_limit")]
		public int? UserLimit;
	}
}
