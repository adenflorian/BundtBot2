using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildMemberRemove
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("user")]
        public DiscordUser RemovedUser;
    }
}