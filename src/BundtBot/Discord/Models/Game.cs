using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class Game
	{
		[JsonProperty("name")]
		public string Name;
	}
}
