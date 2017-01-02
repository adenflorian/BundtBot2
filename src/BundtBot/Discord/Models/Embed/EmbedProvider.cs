using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Embed
{
    public class EmbedProvider
	{
		[JsonProperty("name")]
		public string Name;
		
		[JsonProperty("url")]
		public string Url;
	}
}
