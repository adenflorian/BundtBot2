using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class DmChannel : Channel
    {
		[JsonProperty("recipient")]
		public User Recipient;

		/// <summary>
		/// The id of the last message sent in this DM.
		/// </summary>
		[JsonProperty("last_message_id")]
		public ulong LastMessageId;

		public async Task SendMessage(string message)
		{
			await Client.DiscordRestApiClient.CreateMessageAsync(Id, new CreateMessage {
				Content = message
			});
		}
	}
}
