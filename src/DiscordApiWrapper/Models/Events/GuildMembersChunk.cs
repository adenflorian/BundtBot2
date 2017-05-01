using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildMembersChunk
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("members")]
        public GuildMember[] Members;
    }
}