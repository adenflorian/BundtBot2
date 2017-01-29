using System;
using System.Threading.Tasks;
using BundtBot.Discord;
using BundtBot.Discord.Models;

namespace DiscordApiWrapper.RestApi
{
    public class DiscordRestClientProxy : IDiscordRestClient
    {
        readonly CreateMessageClient _createMsgClient;
		readonly DiscordRestClient _discordRestClient;

        public DiscordRestClientProxy(RestClientConfig config)
        {
			_discordRestClient = new DiscordRestClient(config);
            _createMsgClient = new CreateMessageClient(_discordRestClient);
        }

        public async Task<DiscordMessage> CreateMessageAsync(ulong channelId, CreateMessage createMessage)
        {
            return await _createMsgClient.CreateAsync(channelId, createMessage);
        }

        public async Task<Uri> GetGatewayUrlAsync()
        {
            return await _discordRestClient.GetGatewayUrlAsync();
        }
    }
}