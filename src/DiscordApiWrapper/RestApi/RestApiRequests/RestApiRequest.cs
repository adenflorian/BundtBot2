using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi.RestApiRequests
{
    enum RestRequestType { Get, Post }

    public abstract class RestApiRequest
    {
        /// <summary>Example: channels/123/messages</summary>
        internal abstract string RequestUri { get; }
        internal abstract RestRequestType RequestType { get; }

        internal StringContent BuildContent()
        {
            var body = JsonConvert.SerializeObject(this);
            var content = new StringContent(body);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }
    }
}