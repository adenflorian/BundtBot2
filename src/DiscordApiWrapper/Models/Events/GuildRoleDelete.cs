using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildRoleDelete
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("role_id")]
        public ulong DeletedRoleId;
    }
}