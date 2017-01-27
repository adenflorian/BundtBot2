using System;
using System.Linq;
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
		/// </summary>
		/// <exception cref="DiscordRestException" />
		public async Task<Tuple<DiscordMessage, RateLimit>> CreateMessageAsync(ulong channelId, CreateMessage createMessage)
		{
			var body = JsonConvert.SerializeObject(createMessage);
			var content = new StringContent(body);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
			var response = await PostAsync($"channels/{channelId}/messages", content);

			var rateLimit = new RateLimit(
				int.Parse(response.Headers.GetValues("X-RateLimit-Limit").First()),
				int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()),
				int.Parse(response.Headers.GetValues("X-RateLimit-Reset").First()));

			var message = await DeserializeResponse<DiscordMessage>(response);
			return Tuple.Create(message, rateLimit);
		}

		/// <exception cref="DiscordRestException" />
		async Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			var response = await HttpClient.GetAsync(requestUri);
			if (response.IsSuccessStatusCode == false) {
				var ex = new DiscordRestException("Response did not contain a success status code" +
				                                  ", but instead contained status code " + response.StatusCode);
				_logger.LogError(ex);
				throw ex;
			}
			return response;
		}

		/// <exception cref="DiscordRestException" />
		async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
			var response = await HttpClient.PostAsync(requestUri, content);
			if (response.IsSuccessStatusCode == false) {
				var ex = new DiscordRestException("Response did not contain a success status code" +
												  ", but instead contained status code " + response.StatusCode);
				_logger.LogError(ex);
				throw ex;
			}
			return response;
		}

		/// <exception cref="DiscordRestException" />
		static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			T deserializedObject;
			try {
				deserializedObject = JsonConvert.DeserializeObject<T>(contentString);
			} catch (Exception ex) {
				_logger.LogError(ex);
				throw new DiscordRestException("Error while deserializing json", ex);
			}
			if (deserializedObject == null) {
				var ex = new DiscordRestException(
					"Failed to deserialize object from json" +
					", deserialized object was null" +
					", json content: " + contentString);
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
