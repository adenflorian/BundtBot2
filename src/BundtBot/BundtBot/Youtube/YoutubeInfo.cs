using Newtonsoft.Json;

namespace BundtBot.Youtube
{
    public class YoutubeInfo
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("id")]
        public string Id;
    }
}