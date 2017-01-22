using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Embed
{
    public class EmbedAuthor
    {
		[JsonProperty("name")]
		public string Name;
		
		[JsonProperty("url")]
		public string Url;

		/// <summary>
		/// Url of author icon (only supports http(s) and attachments).
		/// </summary>
		[JsonProperty("icon_url")]
		public string IconUrl;
		
		[JsonProperty("proxy_icon_url")]
		public string ProxyIconUrl;
	}
}
