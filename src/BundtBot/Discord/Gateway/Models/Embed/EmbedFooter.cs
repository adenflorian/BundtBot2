using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class EmbedFooter
	{
		[JsonProperty("text")]
		public string Text;

		/// <summary>
		/// Url of footer icon (only supports http(s) and attachments).
		/// </summary>
		[JsonProperty("icon_url")]
		public string IconUrl;

		[JsonProperty("proxy_icon_url")]
		public string ProxyIconUrl;
	}
}
