using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Extensions;
using BundtCommon;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.RestApi.RestApiRequests;

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

        /// <exception cref="DiscordRestException" />
        /// <exception cref="RateLimitExceededException" />
        public async Task<HttpResponseMessage> ProcessRequestAsync(RestApiRequest request)
        {
            HttpResponseMessage response;
            
            var shortErrors = false;
            //TODO Maybe have it throw after trying for certain amount of time?

            while (true)
            {
                try
                {
                    response = await ActuallySendRequestForReal(request);
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException || ex is TaskCanceledException)
                    {
                        _logger.LogError(ex, shortErrors);
                        shortErrors = true;
                        await _logger.LogAndWaitRetryWarningAsync(TimeSpan.FromSeconds(5));
                    }
                    else throw;
                }
            }
            
            if (response.IsSuccessStatusCode == false)
            {
                await HandleErrorResponseAsync(response);
            }

            return response;
        }

        async Task<HttpResponseMessage> ActuallySendRequestForReal(RestApiRequest request)
        {
            switch (request.RequestType)
            {
                case RestRequestType.Get:
                    return await HttpClient.GetAsync(request.RequestUri);
                case RestRequestType.Post:
                    return await HttpClient.PostAsync(request.RequestUri, request.BuildContent());
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
