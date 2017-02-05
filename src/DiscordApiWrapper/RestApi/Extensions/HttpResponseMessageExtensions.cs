using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Extensions;

namespace DiscordApiWrapper.RestApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        static readonly MyLogger _logger = new MyLogger(nameof(HttpResponseMessageExtensions));
        
		internal static async Task<T> DeserializeResponse<T>(this HttpResponseMessage response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			return contentString.Deserialize<T>();
		}

        internal static DiscordRateLimit GetRateLimit(this HttpResponseMessage response)
		{
			return new DiscordRateLimit(
				GetHeaderIntValue("X-RateLimit-Limit", response.Headers),
				GetHeaderIntValue("X-RateLimit-Remaining", response.Headers),
                DateTimeOffset.FromUnixTimeSeconds(GetHeaderLongValue("X-RateLimit-Reset", response.Headers)).UtcDateTime,
                DateTime.Parse(response.Headers.GetValues("Date").First()).ToUniversalTime());
		}

        static long GetHeaderLongValue(string headerName, HttpResponseHeaders headers)
        {
            return long.Parse(headers.GetValues(headerName).First());
        }

        static int GetHeaderIntValue(string headerName, HttpResponseHeaders headers)
        {
            return int.Parse(headers.GetValues(headerName).First());
        }
    }
}