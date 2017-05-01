using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildRoleCreate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("role")]
        public DiscordRole NewRole;
    }
}