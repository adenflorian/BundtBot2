using System.Net.Http;
using System.Threading.Tasks;
using DiscordApiWrapper.RestApi.RestApiRequests;

namespace DiscordApiWrapper.RestApi
{
    interface IRestRequestProcessor
    {
         Task<HttpResponseMessage> ProcessRequestAsync(RestApiRequest request);
    }
}