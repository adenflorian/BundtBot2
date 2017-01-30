using Newtonsoft.Json;

namespace BundtBot.Discord.Models
{
    public class CreateMessage : IRestApiRequest
    {
		ulong _channelId;

        public string requestUri => $"channels/{_channelId}/messages";

        public RestRequestType requestType => RestRequestType.Post;

        /// <summary>
        /// The message contents (up to 2000 characters).
        /// Required.
        /// </summary>
        [JsonProperty("content")]
        public string Content;

        /// <summary>
        /// A nonce that can be used for optimistic message sending.
        /// Optional.
        /// </summary>
        [JsonProperty("nonce")]
        public ulong? Nonce;

        /// <summary>Optional.</summary>
        [JsonProperty("tts")]
        public bool IsTextToSpeech;

        // TODO
        /// <summary>
        /// The contents of the file being sent.
        /// One of content, file, embeds (multipart form data only).
        /// </summary>
        //[JsonProperty("file")]
        //public ??? file;

        /// <summary>Optional.</summary>
        [JsonProperty("embed")]
        public Embed.Embed EmbeddedContent;

		public CreateMessage(ulong channelId)
		{
            _channelId = channelId;
		}
    }
}
