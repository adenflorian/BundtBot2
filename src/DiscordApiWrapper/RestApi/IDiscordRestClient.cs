using System;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using DiscordApiWrapper.RestApi.RestApiRequests;

namespace DiscordApiWrapper.RestApi
{
    public interface IDiscordRestClient
    {
         Task<Uri> GetGatewayUrlAsync();
         Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content);
    }
}