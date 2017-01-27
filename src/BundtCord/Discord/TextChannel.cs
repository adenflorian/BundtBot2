using System.Threading.Tasks;
using BundtBot.Discord.Models;

namespace BundtBot.Discord
{
    public class TextChannel : ITextChannel
    {
        public ulong Id {get;}
        public string Name {get;}

        DiscordClient _client;

        public TextChannel(GuildChannel guildChannel, DiscordClient client)
        {
            Name = guildChannel.Name;
            Id = guildChannel.Id;
            _client = client;
        }

        public async Task<IMessage> SendMessageAsync(string content)
        {
            var createMessage = new CreateMessage{Content = content};
            var discordMessage = await _client.CreateMsgClient.CreateAsync(Id, createMessage);
            var message = new Message(discordMessage, _client);
            return message;
        }
    }
}
