using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using BundtBot.Extensions;
using DiscordApiWrapper.RestApi;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
	public class DiscordRestClient
	{
		public readonly HttpClient HttpClient;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClient));

		public DiscordRestClient(string botToken, string name, string version, Uri baseAddress)
			: this(botToken, name, version, baseAddress, new HttpClient(new DiscordRestClientLogger(new HttpClientHandler())))
		{
		}

		public DiscordRestClient(string botToken, string name, string version, Uri baseAddress, HttpClient httpClient)
		{
			ValidateArguments(botToken, name, version);

			HttpClient = httpClient;

			InitializeHttpClient(botToken, name, version, baseAddress);
		}

		void InitializeHttpClient(string botToken, string name, string version, Uri baseAddress)
		{
			HttpClient.BaseAddress = baseAddress;
			HttpClient.Timeout = TimeSpan.FromSeconds(1);
			HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);
			HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(name, version));
		}

		static void ValidateArguments(string botToken, string name, string version)
		{
			if (botToken.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(botToken));
			}
			if (name.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(name));
			}
			if (version.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(version));
			}
		}

		/// <exception cref="DiscordRestException" />
		public async Task<Uri> GetGatewayUrlAsync()
		{
			var response = await GetAsync("gateway");
			var gateway = await DeserializeResponse<GatewayUrl>(response);
			return gateway.Url;
		}

		/// <summary>
		/// TODO Requires the 'SEND_MESSAGES' permission to be present on the current user.
		/// TODO Handle HTTP 429
		/// </summary>
		/// <exception cref="DiscordRestException" />
		/// <exception cref="RateLimitExceededException" />
		internal async Task<Tuple<DiscordMessage, RateLimit>> CreateMessageAsync(ulong channelId, CreateMessage createMessage)
		{
			var body = JsonConvert.SerializeObject(createMessage);
			var content = new StringContent(body);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

			var response = await PostAsync($"channels/{channelId}/messages", content);
			
			var message = await DeserializeResponse<DiscordMessage>(response);
			var rateLimit = ParseRateLimitFromHeaders(response);
			return Tuple.Create(message, rateLimit);
		}

		async Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			var response = await HttpClient.GetAsync(requestUri);
			if (response.IsSuccessStatusCode == false) {
				await HandleErrorResponseAsync(response);
			}
			return response;
		}

		async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
			var response = await HttpClient.PostAsync(requestUri, content);
			if (response.IsSuccessStatusCode == false) {
				await HandleErrorResponseAsync(response);
			}
			return response;
		}

        async Task HandleErrorResponseAsync(HttpResponseMessage response)
        {
			Exception ex;
            if (response.StatusCode == (HttpStatusCode)429)
            {
                var rateLimitExceeded = await ParseRateLimitExceededFromResponseAsync(response);
				ex = new RateLimitExceededException(rateLimitExceeded);
            }
			else
			{
            	ex = new DiscordRestException("Received Error Status Code: " + response.StatusCode);
			}
            _logger.LogError(ex);
            throw ex;
        }

        static async Task<RateLimitExceeded> ParseRateLimitExceededFromResponseAsync(HttpResponseMessage response)
        {
            var rateLimitExceeded = await DeserializeResponse<RateLimitExceeded>(response);
            rateLimitExceeded.RateLimit = ParseRateLimitFromHeaders(response);
            rateLimitExceeded.reason = response.ReasonPhrase;
			return rateLimitExceeded;
        }

        static RateLimit ParseRateLimitFromHeaders(HttpResponseMessage response)
		{
			return new RateLimit(
				GetHeaderIntValue("X-RateLimit-Limit", response.Headers),
				GetHeaderIntValue("X-RateLimit-Remaining", response.Headers),
				GetHeaderIntValue("X-RateLimit-Reset", response.Headers));
		}

		static int GetHeaderIntValue(string headerName, HttpResponseHeaders headers)
		{
			return int.Parse(headers.GetValues(headerName).First());
		}

		static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			return Deserialize<T>(contentString);
		}

		static T Deserialize<T>(string content)
		{
			T deserializedObject;

			try {
				deserializedObject = JsonConvert.DeserializeObject<T>(content);
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw new DiscordRestException("Error while deserializing json", ex);
			}
			
			if (deserializedObject == null) {
				var ex = new DiscordRestException($"Deserialized object was null. Json: {content}");
				_logger.LogError(ex);
				throw ex;
			}

			return deserializedObject;
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
				_logger.LogDebug(request);
				if (request.Content != null) {
					_logger.LogDebug(await request.Content.ReadAsStringAsync());
				}

				var response = await base.SendAsync(request, cancellationToken);

				var logResponseMessage = "Response: " + response.StatusCode;
				if (response.IsSuccessStatusCode) {
					_logger.LogInfo(logResponseMessage);
				} else {
					_logger.LogWarning(logResponseMessage);
				}
				_logger.LogDebug(response);
				if (response.Content != null) {
					_logger.LogDebug(await response.Content.ReadAsStringAsync());
				}
				
				return response;
			}
		}
	}
}
