using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class Channel
	{
		internal DiscordClient Client;

		/// <summary>
		/// The id of this channel (will be equal to the guild if it's the "general" channel).
		/// The id of this private message.
		/// </summary>
		[JsonProperty("id")]
		public ulong Id;

		/// <summary>
		/// True for DM Channel, or false for Guild Channel.
		/// </summary>
		[JsonProperty("is_private")]
		public bool IsPrivate;
	}
}
