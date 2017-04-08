using Newtonsoft.Json;

namespace DiscordApiWrapper.Gateway.Models
{
    public class GatewayVoiceStateUpdate
    {
        [JsonProperty("guild_id")]
        public ulong GuildId;

        /// <summary>
        /// Voice channel to join, or null to leave voice channel
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
        public ulong? VoiceChannelId;

        [JsonProperty("self_deaf")]
        public bool IsDeafenedBySelf;

        [JsonProperty("self_mute")]
        public bool IsMutedBySelf;
    }
}