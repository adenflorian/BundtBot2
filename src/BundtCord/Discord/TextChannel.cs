using System.Threading.Tasks;
using BundtBot.Discord.Models;
using DiscordApiWrapper.RestApi.RestApiRequests;

namespace BundtCord.Discord
{
    public class TextChannel : ITextChannel
    {
        public ulong Id {get;}
        public string Name {get;}
        public ulong ServerId {get;}

        DiscordClient _client;

        public TextChannel(GuildChannel guildChannel, DiscordClient client)
        {
            Name = guildChannel.Name;
            Id = guildChannel.Id;
            ServerId = guildChannel.GuildID;
            _client = client;
        }

        public async Task<IMessage> SendMessageAsync(string content)
        {
            var createMessage = new NewMessageRequest(Id){Content = content};
            var discordMessage = await _client.DiscordRestClient.CreateMessageAsync(createMessage);
            var message = new Message(discordMessage, _client);
            return message;
        }
    }
}
