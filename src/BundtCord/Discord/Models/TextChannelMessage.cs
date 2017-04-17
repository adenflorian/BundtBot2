using System.Threading.Tasks;
using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    public class TextChannelMessage
    {
        public string Content { get; }
        public ServerMember Author => _client.ServerMembers[_serverId][_authorId];
        public TextChannel TextChannel => _client.TextChannels[_textChannelId];

        readonly ulong _serverId;
        readonly ulong _authorId;
        readonly ulong _textChannelId;
        readonly DiscordClient _client;

        public TextChannelMessage(DiscordMessage discordMessage, ulong serverId, DiscordClient client)
        {
            Content = discordMessage.Content;
            _textChannelId = discordMessage.ChannelId;
            _authorId = discordMessage.Author.Id;
            _serverId = serverId;
            _client = client;
        }

        public async Task ReplyAsync(string messageContent)
        {
            await TextChannel.SendMessageAsync(messageContent);
        }
    }
}
