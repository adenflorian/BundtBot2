using System.Net.Http;
using System.Threading.Tasks;
using DiscordApiWrapper.RestApi.RestApiRequests;

namespace DiscordApiWrapper.RestApi
{
    public interface IRestRequestProcessor
    {
         Task<HttpResponseMessage> ProcessRequestAsync(IRestApiRequest request);
    }
}