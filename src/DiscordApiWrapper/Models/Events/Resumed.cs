using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class Resumed
    {
        [JsonProperty("_trace")]
        public string[] ConnectedServers;
    }
}