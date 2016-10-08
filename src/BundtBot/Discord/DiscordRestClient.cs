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
		const string LogPrefix = "API: ";

		public DiscordRestClient(string botToken, string name, string version)
			: base(new DiscordRestClientLogger(new HttpClientHandler()))
		{
			if (botToken.IsNullOrWhiteSpace()) {
				throw new ArgumentException(nameof(botToken));
			}
			BaseAddress = new Uri("https://discordapp.com/api/");
			Timeout = TimeSpan.FromSeconds(1);
			SetHeaders(botToken, name, version);
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

			public DiscordRestClientLogger(HttpMessageHandler innerHandler)
				: base(innerHandler)
			{
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
				CancellationToken cancellationToken)
			{

				_logger.LogInfo(LogPrefix + "Request: " + request.RequestUri);
				_logger.LogDebug(LogPrefix + request);
				if (request.Content != null) {
					_logger.LogDebug(LogPrefix + await request.Content.ReadAsStringAsync());
				}

				var response = await base.SendAsync(request, cancellationToken);

				_logger.LogInfo(LogPrefix + "Response: " + response.StatusCode);
				_logger.LogDebug(LogPrefix + response);
				if (response.Content != null) {
					_logger.LogDebug(LogPrefix + await response.Content.ReadAsStringAsync());
				}

				return response;
			}
		}
	}
}
