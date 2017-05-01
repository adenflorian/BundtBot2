using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildMemberUpdate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("roles")]
        public ulong[] RoleIds;

        [JsonProperty("user")]
        public DiscordUser User;

        /// <summary>
        /// This users guild nickname (if one is set).
        /// </summary>
        [JsonProperty("nick")]
        public string Nickname;
    }
}