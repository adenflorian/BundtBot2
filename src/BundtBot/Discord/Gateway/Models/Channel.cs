using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Models;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class Channel
    {
		/// <summary>
		/// The id of this channel (will be equal to the guild if it's the "general" channel).
		/// Present: Always.
		/// </summary>
		[JsonProperty("id")]
		public ulong ID;

		/// <summary>
		/// Present: Always.
		/// </summary>
		[JsonProperty("guild_id")]
		public ulong GuildID;

		/// <summary>
		/// 2-100 characters.
		/// Present: Always.
		/// </summary>
		[JsonProperty("name")]
		public string Name;

		/// <summary>
		/// "text" or "voice".
		/// Present: Always.
		/// </summary>
		[JsonProperty("type")]
		public string Type;

		/// <summary>
		/// Sorting position of the channel.
		/// Present: Always.
		/// </summary>
		[JsonProperty("position")]
		public int Position;

		/// <summary>
		/// Should always be false for guild channels.
		/// Present: Always.
		/// </summary>
		[JsonProperty("is_private")]
		public bool IsPrivate;

		/// <summary>
		/// Present: Always.
		/// </summary>
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

	    public async Task SendMessage(DiscordClient client, string message)
	    {
			await client.DiscordRestApiClient.CreateMessageAsync(ID, new CreateMessage {
				Content = message
			});
	    }
	}
}
