using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Embed
{
    public class EmbedThumbnail
	{
		/// <summary>
		/// Source url of thumbnail (only supports http(s) and attachments).
		/// </summary>
		[JsonProperty("url")]
		public string SourceUrl;

		[JsonProperty("proxy_icon_url")]
		public string ProxySourceUrl;

		[JsonProperty("height")]
		public int Height;

		[JsonProperty("width")]
		public int Width;
	}
}
