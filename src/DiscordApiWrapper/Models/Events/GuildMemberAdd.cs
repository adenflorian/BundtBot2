using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildMemberAdd : GuildMember
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;
    }
}