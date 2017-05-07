using System;
using BundtBot.Discord.Models;

namespace BundtCord.Discord
{
    public class ServerMember
    {
        public readonly DateTime JoinedAt;
        public bool IsDeafened { get; internal set; }
        public bool IsMuted { get; internal set; }
        /// <summary>
        /// Member's nickname, or null if they don't have one
        /// </summary>
        public string Nickname { get; internal set; }
        public ulong[] RoleIds { get; internal set; }

        public Server Server => _client.Servers[_serverId];
        public User User => _client.Users[_userId];
        public VoiceChannel VoiceChannel => VoiceChannelId.HasValue ? _client.VoiceChannels[VoiceChannelId.Value] : null;

        internal ulong? VoiceChannelId;
        
        ulong _serverId;
        ulong _userId;
        DiscordClient _client;

        public ServerMember(GuildMember guildMember, ulong serverId, DiscordClient client)
        {
            _serverId = serverId;
            _userId = guildMember.User.Id;
            _client = client;
            JoinedAt = guildMember.JoinedAt;
            IsDeafened = guildMember.IsDeafened;
            IsMuted = guildMember.IsMuted;
            Nickname = guildMember.Nickname;
            RoleIds = guildMember.RoleIds;
        }

        public override string ToString()
        {
            var str = "{ ";

            str += nameof(_serverId) + ": " + _serverId;
            str += ", ";
            str += nameof(_userId) + ": " + _userId;
            str += ", ";
            str += nameof(JoinedAt) + ": " + JoinedAt;
            str += ", ";
            str += nameof(VoiceChannel) + ": " + (VoiceChannel != null ? VoiceChannel.ToString() : "null");
            str += ", ";
            str += nameof(IsDeafened) + ": " + IsDeafened;
            str += ", ";
            str += nameof(IsMuted) + ": " + IsMuted;
            str += ", ";
            str += nameof(Nickname) + ": " + (Nickname != null ? Nickname : "null");
            str += ", ";
            str += nameof(RoleIds) + ": " + RoleIds.Length + " roles";
            str += " }";

            return str;
        }
    }
}