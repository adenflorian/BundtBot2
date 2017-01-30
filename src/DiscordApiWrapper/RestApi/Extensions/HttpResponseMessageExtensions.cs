using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BundtBot.Extensions;

namespace DiscordApiWrapper.RestApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
		internal static async Task<T> DeserializeResponse<T>(this HttpResponseMessage response)
		{
			var contentString = await response.Content.ReadAsStringAsync();
			return contentString.Deserialize<T>();
		}

        internal static RateLimit GetRateLimit(this HttpResponseMessage response)
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
    }
}