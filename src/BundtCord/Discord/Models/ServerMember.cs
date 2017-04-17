namespace BundtCord.Discord
{
    public class ServerMember
    {
        public Server Server => _client.Servers[_serverId];
        public User User => _client.Users[_userId];
        public VoiceChannel VoiceChannel => VoiceChannelId.HasValue ? _client.VoiceChannels[VoiceChannelId.Value] : null;

        internal ulong? VoiceChannelId;
        
        ulong _serverId;
        ulong _userId;
        DiscordClient _client;

        public ServerMember(ulong serverId, ulong userId, DiscordClient client)
        {
            _serverId = serverId;
            _userId = userId;
            _client = client;
        }
    }
}