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
        readonly RateLimitedClient _rateLimitedClient;

        public DiscordRestClientProxy(RestClientConfig config)
        {
            _rateLimitedClient = new RateLimitedClient(new DiscordRestClient(config));
        }

        public async Task<DiscordMessage> CreateMessageAsync(NewMessageRequest createMessage)
        {
            return await DoRequestAsync<DiscordMessage>(createMessage);
        }

        public async Task<Uri> GetGatewayUrlAsync()
        {
            return (await DoRequestAsync<GatewayUrl>(new GetRequest("gateway"))).Url;
        }

        async Task<T> DoRequestAsync<T>(IRestApiRequest request)
        {
            var response = await _rateLimitedClient.ProcessRequestAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return content.Deserialize<T>();
        }
    }
}