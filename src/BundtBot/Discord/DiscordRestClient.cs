using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Extensions;
using Newtonsoft.Json.Linq;

namespace BundtBot.Discord
{
	public class DiscordRestClient : HttpClient
	{
		public DiscordRestClient(string botToken, string name, string version)
			: base(new DiscordRestClientLogger(new HttpClientHandler()))
		{
			ValidateArguments(botToken, name, version);

			BaseAddress = new Uri("https://discordapp.com/api/");
			Timeout = TimeSpan.FromSeconds(1);
			SetHeaders(botToken, name, version);
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

		void SetHeaders(string botToken, string name, string version)
		{
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);
			DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(name, version));
		}

		public Uri GetGatewayUrl()
		{
			var response = GetAsync("gateway").Result.Content.ReadAsStringAsync().Result;
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
