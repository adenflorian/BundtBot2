using System;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Models;
using BundtBot.Extensions;
using DiscordApiWrapper.RestApi.RestApiRequests;

namespace DiscordApiWrapper.RestApi
{
    public class DiscordRestClientProxy : IDiscordRestClient
    {
        readonly RateLimitedClient _createMessageClient;
        readonly RateLimitedClient _getGatewayUrlCient;

        public DiscordRestClientProxy(RestClientConfig config)
        {
            var discordRestClient = new DiscordRestClient(config);
            _createMessageClient = new RateLimitedClient(discordRestClient);
            _getGatewayUrlCient = new RateLimitedClient(discordRestClient);
        }
        
        /// <summary>
        /// /// TODO Requires the 'SEND_MESSAGES' permission to be present on the current user.
        /// </summary>
        public async Task<DiscordMessage> CreateMessageAsync(NewMessageRequest createMessage)
        {
            return await DoRequestAsync<DiscordMessage>(createMessage, _createMessageClient);
        }

        public async Task<Uri> GetGatewayUrlAsync()
        {
            return (await DoRequestAsync<GatewayUrl>(new GetRequest("gateway"), _getGatewayUrlCient)).Url;
        }

        async Task<T> DoRequestAsync<T>(RestApiRequest request, IRestRequestProcessor processor)
        {
            var response = await processor.ProcessRequestAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return content.Deserialize<T>();
        }
    }
}