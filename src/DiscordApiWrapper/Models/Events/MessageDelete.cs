using Newtonsoft.Json;

namespace DiscordApiWrapper.Models.Events
{
    public class MessageDelete
    {
        [JsonProperty("id")]
        public ulong DeletedMessageId;

        [JsonProperty("channel_id")]
        public ulong ChannelId;
    }
}