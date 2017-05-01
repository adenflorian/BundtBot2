using BundtBot.Discord.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class GuildEmojisUpdate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        [JsonProperty("emojis")]
        public Emoji[] Emojis;
    }
}