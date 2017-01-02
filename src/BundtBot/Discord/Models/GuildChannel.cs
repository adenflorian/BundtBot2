using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
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
		public string Type;

		/// <summary>
		/// Sorting position of the channel.
		/// </summary>
		[JsonProperty("position")]
		public int Position;
		
		[JsonProperty("permission_overwrites")]
		public Overwrite[] PermissionOverwrites;
	}
}
