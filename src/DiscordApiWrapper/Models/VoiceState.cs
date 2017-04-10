using Newtonsoft.Json;

namespace DiscordApiWrapper.Models
{
    public class VoiceState
    {
        [JsonProperty("guild_id")]
		public ulong? GuildID;

        [JsonProperty("channel_id")]
        public ulong? ChannelId;

        [JsonProperty("user_id")]
        public ulong UserId;

		[JsonProperty("session_id")]
        public string SessionId;

        [JsonProperty("deaf")]
        public bool IsDeafenedByServer;

        [JsonProperty("mute")]
        public bool IsMutedByServer;

        [JsonProperty("self_deaf")]
        public bool IsDeafenedBySelf;

        [JsonProperty("self_mute")]
        public bool IsMutedBySelf;

        /// <summary>Whether this user is muted by the current user</summary>
        [JsonProperty("suppress")]
        public bool IsMutedByMe;
    }
}
