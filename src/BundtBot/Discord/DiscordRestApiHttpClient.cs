using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BundtBot.Discord {
	public class DiscordRestApiHttpClient : HttpClient {
		static readonly string _logPrefix = "API: ";

		public DiscordRestApiHttpClient(string botToken, string name, string version) : base(new LoggingHandler(new HttpClientHandler())) {
			BaseAddress = new Uri("https://discordapp.com/api/");
			Timeout = TimeSpan.FromSeconds(1);
			SetHeaders(botToken, name, version);
		}

		void SetHeaders(string botToken, string name, string version) {
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);
			DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(name, version));
		}

		public Uri GetGatewayUrl() {
			var response = GetAsync("gateway").Result.Content.ReadAsStringAsync().Result;
			var url = JObject.Parse(response)["url"];
			return new Uri(url.ToString());
		}

		class LoggingHandler : DelegatingHandler {
			public LoggingHandler(HttpMessageHandler innerHandler)
				: base(innerHandler) {
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken) {
				MyLogger.LogInfo(_logPrefix + "Request: " + request.RequestUri);
				MyLogger.LogDebug(_logPrefix + request);
				if (request.Content != null) {
					MyLogger.LogDebug(_logPrefix + await request.Content.ReadAsStringAsync());
				}

				var response = await base.SendAsync(request, cancellationToken);

				MyLogger.LogInfo(_logPrefix + "Response: " + response.StatusCode);
				MyLogger.LogDebug(_logPrefix + response);
				if (response.Content != null) {
					MyLogger.LogDebug(_logPrefix + await response.Content.ReadAsStringAsync());
				}

				return response;
			}
		}
	}
}
