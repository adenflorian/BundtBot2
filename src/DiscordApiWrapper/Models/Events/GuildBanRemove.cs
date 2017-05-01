using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildBanRemove : DiscordUser
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;
    }
}