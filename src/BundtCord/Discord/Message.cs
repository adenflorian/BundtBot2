using BundtBot.Discord.Models;

namespace BundtBot.Discord
{
    public class Message : IMessage
    {
        public IUser Author { get; }
        public string Content { get; }

        public ITextChannel TextChannel { get; }

        public Message(DiscordMessage discordMessage, DiscordClient client)
        {
            Content = discordMessage.Content;
            TextChannel = client.TextChannels[discordMessage.ChannelId];
            Author = client.Users[discordMessage.Author.Id];
        }
    }
}
