using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class DmChannel : Channel
    {
		[JsonProperty("recipient")]
		public DiscordUser Recipient;

		/// <summary>
		/// The id of the last message sent in this DM.
		/// </summary>
		[JsonProperty("last_message_id")]
		public ulong LastMessageId;
	}
}
