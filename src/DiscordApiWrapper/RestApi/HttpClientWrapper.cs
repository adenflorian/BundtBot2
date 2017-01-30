using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using BundtBot.Extensions;

namespace DiscordApiWrapper.RestApi
{
    public class HttpClientWrapper
    {
        internal readonly HttpClient HttpClient;

        static readonly MyLogger _logger = new MyLogger(nameof(HttpClientWrapper));

        internal HttpClientWrapper(RestClientConfig config, HttpClient httpClient = null)
        {
			if (httpClient == null)
			{
				httpClient = new HttpClient(new DiscordRestClientLogger(new HttpClientHandler()));
			}

            HttpClient = httpClient;
            
			ValidateArguments(config);

            InitializeHttpClient(config);
        }

		static void ValidateArguments(RestClientConfig config)
		{
			if (config.BotToken.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(config.BotToken));
			}
			if (config.Name.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(config.Name));
			}
			if (config.Version.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(config.Version));
			}
		}

        void InitializeHttpClient(RestClientConfig config)
        {
            HttpClient.BaseAddress = config.BaseAddress;
            HttpClient.Timeout = TimeSpan.FromSeconds(1);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", config.BotToken);
            HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(config.Name, config.Version));
        }

        internal async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var response = await HttpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode == false)
            {
                await HandleErrorResponseAsync(response);
            }
            return response;
        }

        internal async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            var response = await HttpClient.PostAsync(requestUri, content);
            if (response.IsSuccessStatusCode == false)
            {
                await HandleErrorResponseAsync(response);
            }
            return response;
        }

        async Task HandleErrorResponseAsync(HttpResponseMessage response)
        {
            Exception ex;
            if (response.StatusCode == (HttpStatusCode)429)
            {
                var rateLimitExceeded = await RestApiHelper.ParseRateLimitExceededFromResponseAsync(response);
                ex = new RateLimitExceededException(rateLimitExceeded);
            }
            else
            {
                ex = new DiscordRestException("Received Error Status Code: " + response.StatusCode);
            }
            _logger.LogError(ex);
            throw ex;
        }
    }

    class DiscordRestClientLogger : DelegatingHandler
    {
        readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClientLogger));

        public DiscordRestClientLogger(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Request: " + request.RequestUri);
            _logger.LogTrace(request);
            if (request.Content != null)
            {
                _logger.LogTrace(await request.Content.ReadAsStringAsync());
            }

            var response = await base.SendAsync(request, cancellationToken);

            var logResponseMessage = "Response: " + response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo(logResponseMessage);
            }
            else
            {
                _logger.LogWarning(logResponseMessage);
            }
            _logger.LogTrace(response);
            if (response.Content != null)
            {
                _logger.LogTrace(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}