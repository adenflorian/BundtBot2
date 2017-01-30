using System;
using System.Threading.Tasks;
using BundtBot.Discord.Models;

namespace DiscordApiWrapper.RestApi
{
    public interface IDiscordRestClient
    {
         Task<Uri> GetGatewayUrlAsync();
         Task<DiscordMessage> CreateMessageAsync(CreateMessage createMessage);
    }
}