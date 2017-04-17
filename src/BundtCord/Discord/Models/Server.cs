using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using DiscordApiWrapper.Voice;

namespace BundtCord.Discord
{
    public class Server
    {
        public ulong Id { get; }
        public IEnumerable<TextChannel> TextChannels => _client.TextChannels.Values.Where(x => x.ServerId == Id);
        public IEnumerable<VoiceChannel> VoiceChannels => _client.VoiceChannels.Values.Where(x => x.ServerId == Id);
        public IEnumerable<ServerMember> Members => _client.ServerMembers[Id].Values;
        public DiscordVoiceClient VoiceClient { get; internal set; }

        DiscordClient _client;

        public Server(DiscordGuild discordGuild, DiscordClient client)
        {
            Id = discordGuild.Id;
            _client = client;
        }

        public async Task LeaveVoice()
        {
            await _client.LeaveVoiceChannelInServer(this);
        }
    }
}