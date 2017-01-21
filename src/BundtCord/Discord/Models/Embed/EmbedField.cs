using Newtonsoft.Json;

namespace BundtBot.Discord.Models.Embed
{
    public class EmbedField
	{
		[JsonProperty("name")]
		public string Name;

		[JsonProperty("value")]
		public string Value;
		
		[JsonProperty("inline")]
		public bool ShouldDisplayInline;
	}
}
