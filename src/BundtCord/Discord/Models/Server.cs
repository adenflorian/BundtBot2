using System.Collections.Generic;
using System.Linq;
using BundtBot.Discord.Models;
using DiscordApiWrapper.Voice;

namespace BundtCord.Discord
{
    public class Server : IServer
    {
        public ulong Id { get; }
        public IEnumerable<ITextChannel> TextChannels => _client.TextChannels.Values.Where(x => x.ServerId == Id);
        public IEnumerable<IVoiceChannel> VoiceChannels => _client.VoiceChannels.Values.Where(x => x.ServerId == Id);
        public IEnumerable<IServerMember> Members => _client.ServerMembers[Id].Values;
        public DiscordVoiceClient VoiceClient { get; internal set; }

        DiscordClient _client;

        public Server(DiscordGuild discordGuild, DiscordClient client)
        {
            Id = discordGuild.Id;
            _client = client;
        }
    }
}