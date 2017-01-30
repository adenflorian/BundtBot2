using System;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Models;

namespace DiscordApiWrapper.RestApi
{
    public class DiscordRestClientProxy : IDiscordRestClient
    {
        readonly RateLimitedClient _createMsgClient;
		readonly DiscordRestClient _discordRestClient;

        public DiscordRestClientProxy(RestClientConfig config)
        {
			_discordRestClient = new DiscordRestClient(config);
            _createMsgClient = new RateLimitedClient(_discordRestClient);
        }

        public async Task<DiscordMessage> CreateMessageAsync(CreateMessage createMessage)
        {
            var response = await _createMsgClient.CreateAsync(createMessage);
            return RestApiHelper.Deserialize<DiscordMessage>(response);
        }

        public async Task<Uri> GetGatewayUrlAsync()
        {
            return await _discordRestClient.GetGatewayUrlAsync();
        }
    }
}