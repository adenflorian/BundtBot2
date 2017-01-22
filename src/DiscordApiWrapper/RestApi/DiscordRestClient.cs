using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using BundtBot.Extensions;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
	public class DiscordRestClient
	{
		public readonly HttpClient HttpClient;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClient));

		public DiscordRestClient(string botToken, string name, string version)
			: this(botToken, name, version, new HttpClient(new DiscordRestClientLogger(new HttpClientHandler())))
		{
		}

		public DiscordRestClient(string botToken, string name, string version, HttpClient httpClient)
		{
			ValidateArguments(botToken, name, version);

			HttpClient = httpClient;

			InitializeHttpClient(botToken, name, version);
		}

		void InitializeHttpClient(string botToken, string name, string version)
		{
			HttpClient.BaseAddress = new Uri("https://discordapp.com/api/");
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
		public async Task<Message> CreateMessageAsync(ulong channelId, CreateMessage createMessage)
		{
			var body = JsonConvert.SerializeObject(createMessage);
			var content = new StringContent(body);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
			var response = await PostAsync($"channels/{channelId}/messages", content);
			var message = await DeserializeResponse<Message>(response);
			return message;
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
