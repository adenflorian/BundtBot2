using System.Collections.Generic;
using System.Linq;
using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    public class Server : IServer
    {
        public ulong Id { get; }

        public IEnumerable<ITextChannel> TextChannels
        {
            get
            {
                return _client.TextChannels.Values.Where(x => x.ServerId == Id);
            }
        }

        public IEnumerable<VoiceChannel> VoiceChannels
        {
            get
            {
                return _client.VoiceChannels.Values.Where(x => x.ServerId == Id);
            }
        }

        DiscordClient _client;

        public Server(DiscordGuild discordGuild, DiscordClient client)
        {
            Id = discordGuild.Id;
            _client = client;
        }
    }
}