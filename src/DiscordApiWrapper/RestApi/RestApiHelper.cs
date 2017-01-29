using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord;
using Newtonsoft.Json;

namespace DiscordApiWrapper.RestApi
{
    public static class RestApiHelper
    {
		static readonly MyLogger _logger = new MyLogger(nameof(RestApiHelper));

        internal static async Task<RateLimitExceeded> ParseRateLimitExceededFromResponseAsync(HttpResponseMessage response)
        {
            var rateLimitExceeded = await DeserializeResponse<RateLimitExceeded>(response);
            rateLimitExceeded.RateLimit = ParseRateLimitFromHeaders(response);
            rateLimitExceeded.reason = response.ReasonPhrase;
			return rateLimitExceeded;
        }

        internal static RateLimit ParseRateLimitFromHeaders(HttpResponseMessage response)
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

		internal static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
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
    }
}