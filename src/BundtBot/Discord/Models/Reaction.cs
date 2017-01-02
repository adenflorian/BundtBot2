using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class Reaction
	{
		/// <summary>
		/// Times this emoji has been used to react.
		/// </summary>
		[JsonProperty("count")]
		public int Count;

		/// <summary>
		/// whether the current user reacted using this emoji
		/// </summary>
		[JsonProperty("me")]
		public bool Me;
		
		[JsonProperty("emoji")]
		public Emoji Emoji;
	}
}
