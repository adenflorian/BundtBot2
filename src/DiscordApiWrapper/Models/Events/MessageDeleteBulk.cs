using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class MessageDeleteBulk
    {
        [JsonProperty("ids")]
        public ulong[] DeletedMessageIds;

        [JsonProperty("channel_id")]
        public ulong ChannelId;
    }
}