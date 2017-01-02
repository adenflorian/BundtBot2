using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class TextChannel : GuildChannel
    {
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
		
	    public async Task SendMessage(string message)
	    {
			await Client.DiscordRestApiClient.CreateMessageAsync(Id, new CreateMessage {
				Content = message
			});
	    }
	}
}
