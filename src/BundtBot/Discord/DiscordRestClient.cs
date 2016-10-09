using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Extensions;
using Newtonsoft.Json.Linq;

namespace BundtBot.Discord
{
	public class DiscordRestClient
	{
		public readonly HttpClient HttpClient;

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

		public Uri GetGatewayUrl()
		{
			var response = HttpClient.GetAsync("gateway").Result.Content.ReadAsStringAsync().Result;
			var url = JObject.Parse(response)["url"];
			return new Uri(url.ToString());
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

				_logger.LogInfo("Response: " + response.StatusCode);
				_logger.LogDebug(response);
				if (response.Content != null) {
					_logger.LogDebug(await response.Content.ReadAsStringAsync());
				}

				return response;
			}
		}
	}
}
