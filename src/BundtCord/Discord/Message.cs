using System;
using BundtBot.Discord.Models;

namespace BundtBot.Discord
{
    public class Message : IMessage
    {
        public string Content;

        public ITextChannel TextChannel { get; }

        public Message(DiscordMessage discordMessage, ITextChannel textChannel)
        {
            Content = discordMessage.Content;
            TextChannel = textChannel;
        }
    }
}
