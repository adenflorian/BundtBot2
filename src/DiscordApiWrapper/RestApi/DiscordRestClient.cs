using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.RestApi.RestApiRequests;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
    class DiscordRestClient : IRestRequestProcessor
    {
        readonly HttpClientWrapper _httpClientWrapper;

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClient));

        public DiscordRestClient(RestClientConfig config, HttpClient httpClient = null)
        {
            _httpClientWrapper = new HttpClientWrapper(config, httpClient);
        }

        /// <summary>
        /// TODO Requires the 'SEND_MESSAGES' permission to be present on the current user.
        /// </summary>
        /// <exception cref="DiscordRestException" />
        /// <exception cref="RateLimitExceededException" />
        public async Task<HttpResponseMessage> ProcessRequestAsync(IRestApiRequest request)
        {
            switch (request.RequestType)
            {
                case RestRequestType.Get: return await GetRequestAsync(request);
                case RestRequestType.Post: return await PostRequestAsync(request);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        async Task<HttpResponseMessage> GetRequestAsync(IRestApiRequest request)
        {
            return await _httpClientWrapper.GetAsync(request.RequestUri);
        }

        async Task<HttpResponseMessage> PostRequestAsync(IRestApiRequest request)
        {
            var content = BuildContent(request);
            return await _httpClientWrapper.PostAsync(request.RequestUri, content);
        }

        StringContent BuildContent(IRestApiRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            var content = new StringContent(body);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }
    }
}
