using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildDeleted
    {
        [JsonProperty("id")]
        public ulong DeletedGuildId;

        [JsonProperty("unavailable")]
        public bool IsUnavaliable;
    }
}