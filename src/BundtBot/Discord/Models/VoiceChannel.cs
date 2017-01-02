using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class VoiceChannel : GuildChannel
	{
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
