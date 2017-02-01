using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BundtBot.Extensions;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.RestApi.RestApiRequests;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
    class DiscordRestClient : IRestRequestProcessor
    {
        internal readonly HttpClient HttpClient;

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClient), ConsoleColor.Magenta);

        public DiscordRestClient(RestClientConfig config, HttpClient httpClient = null)
        {
            HttpClient = httpClient ?? new HttpClient(new DiscordRestClientLogger(new HttpClientHandler()));
            ValidateArguments(config);
            InitializeHttpClient(config);
        }

        static void ValidateArguments(RestClientConfig config)
        {
            if (config.BotToken.IsNullOrWhiteSpace()) throw new ArgumentException(nameof(config.BotToken));
            if (config.Name.IsNullOrWhiteSpace()) throw new ArgumentException(nameof(config.Name));
            if (config.Version.IsNullOrWhiteSpace()) throw new ArgumentException(nameof(config.Version));
        }

        void InitializeHttpClient(RestClientConfig config)
        {
            HttpClient.BaseAddress = config.BaseAddress;
            HttpClient.Timeout = TimeSpan.FromSeconds(1);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", config.BotToken);
            HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(config.Name, config.Version));
        }

        /// <summary>
        /// TODO Requires the 'SEND_MESSAGES' permission to be present on the current user.
        /// </summary>
        /// <exception cref="DiscordRestException" />
        /// <exception cref="RateLimitExceededException" />
        public async Task<HttpResponseMessage> ProcessRequestAsync(IRestApiRequest request)
        {
            HttpResponseMessage response;

            switch (request.RequestType)
            {
                case RestRequestType.Get:
                    response = await HttpClient.GetAsync(request.RequestUri);
                    break;
                case RestRequestType.Post:
                    response = await HttpClient.PostAsync(request.RequestUri, BuildContent(request));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (response.IsSuccessStatusCode == false)
            {
                await HandleErrorResponseAsync(response);
            }

            return response;
        }

        StringContent BuildContent(IRestApiRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            var content = new StringContent(body);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        async Task HandleErrorResponseAsync(HttpResponseMessage response)
        {
            Exception ex;
            if (response.StatusCode == (HttpStatusCode)429)
            {
                ex = new RateLimitExceededException(await RateLimitExceeded.Create(response));
            }
            else
            {
                ex = new DiscordRestException("Received Error Status Code: " + response.StatusCode);
            }
            _logger.LogError(ex);
            throw ex;
        }
    }
}
