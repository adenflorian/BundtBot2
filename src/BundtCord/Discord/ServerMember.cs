namespace BundtCord.Discord
{
    public class ServerMember : IServerMember
    {
        public IServer Server => _client.Servers[_serverId];
        public IUser User => _client.Users[_userId];
        public IVoiceChannel VoiceChannel => VoiceChannelId.HasValue ? _client.VoiceChannels[VoiceChannelId.Value] : null;

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