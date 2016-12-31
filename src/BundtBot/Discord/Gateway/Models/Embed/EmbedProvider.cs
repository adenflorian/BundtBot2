using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway.Models
{
    public class EmbedProvider
	{
		[JsonProperty("name")]
		public string Name;
		
		[JsonProperty("url")]
		public string Url;
	}
}
