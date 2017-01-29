using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using DiscordApiWrapper.RestApi;
using Newtonsoft.Json;

namespace BundtBot.Discord
{
	public class DiscordRestClient
	{
		readonly HttpClientWrapper _httpClientWrapper;

		static readonly MyLogger _logger = new MyLogger(nameof(DiscordRestClient));

		public DiscordRestClient(RestClientConfig config, HttpClient httpClient = null)
		{
			_httpClientWrapper = new HttpClientWrapper(config, httpClient);
		}

		/// <exception cref="DiscordRestException" />
		public async Task<Uri> GetGatewayUrlAsync()
		{
			var response = await _httpClientWrapper.GetAsync("gateway");
			var gateway = await RestApiHelper.DeserializeResponse<GatewayUrl>(response);
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

			var response = await _httpClientWrapper.PostAsync($"channels/{channelId}/messages", content);
			
			var message = await RestApiHelper.DeserializeResponse<DiscordMessage>(response);
			var rateLimit = RestApiHelper.ParseRateLimitFromHeaders(response);
			return Tuple.Create(message, rateLimit);
		}
	}
}
