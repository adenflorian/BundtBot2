using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    public class VoiceChannel
    {
        public ulong Id { get; }
        public string Name { get; }
        public ulong ServerId { get; }

        public VoiceChannel(GuildChannel guildChannel)
        {
            Id = guildChannel.Id;
            Name = guildChannel.Name;
            ServerId = guildChannel.GuildID;
        }
    }
}