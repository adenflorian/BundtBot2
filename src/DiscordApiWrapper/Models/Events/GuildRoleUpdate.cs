using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildRoleUpdate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("role")]
        public DiscordRole NewRole;
    }
}