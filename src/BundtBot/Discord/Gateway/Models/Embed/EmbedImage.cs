using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class EmbedImage
	{
		/// <summary>
		/// Source url of image (only supports http(s) and attachments).
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
