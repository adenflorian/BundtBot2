using System.IO;
using System.Threading.Tasks;
using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    public class VoiceChannel
    {
        public ulong Id { get; }
        public string Name { get; }
        public ulong ServerId { get; }

        public Server Server => _client.Servers[ServerId];

        DiscordClient _client;

        public VoiceChannel(GuildChannel guildChannel, DiscordClient client)
        {
            Id = guildChannel.Id;
            Name = guildChannel.Name;
            ServerId = guildChannel.GuildID;
            _client = client;
        }

        public async Task JoinAsync()
        {
            await _client.JoinVoiceChannel(this);
        }

        public async Task LeaveAsync()
        {
            await _client.LeaveVoiceChannelInServer(Server);
        }

        public async Task SendAudioAsync(Stream pcmAudioStream)
        {
            await Server.VoiceClient.SendAudioAsync(pcmAudioStream);
        }
    }
}