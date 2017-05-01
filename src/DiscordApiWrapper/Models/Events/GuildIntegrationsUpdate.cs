using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildIntegrationsUpdate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;
    }
}