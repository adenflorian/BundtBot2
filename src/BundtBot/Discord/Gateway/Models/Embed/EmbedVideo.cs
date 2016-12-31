using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class EmbedVideo
	{
		[JsonProperty("url")]
		public string SourceUrl;

		[JsonProperty("height")]
		public int Height;

		[JsonProperty("width")]
		public int Width;
	}
}
