using System.Threading.Tasks;
using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    class VoiceChannel : IVoiceChannel
    {
        public ulong Id { get; }
        public string Name { get; }
        public ulong ServerId { get; }

        public IServer Server => _client.Servers[ServerId];

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
            await _client.LeaveVoiceChannelInServer(ServerId);
        }

        public async Task SendAudioAsync(byte[] sodaBytes)
        {
            await Server.VoiceClient.SendAudioAsync(sodaBytes);
        }
    }
}