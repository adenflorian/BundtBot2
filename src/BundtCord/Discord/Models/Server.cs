using System;
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
        public TimeSpan AfkTimeout { get; internal set; }
        public IEnumerable<TextChannel> TextChannels => _client.TextChannels.Values.Where(x => x.ServerId == Id);
        public VoiceChannel AfkChannel => _afkChannelId.HasValue ? _client.VoiceChannels.Values.First(x => x.Id == _afkChannelId) : null;
        public IEnumerable<VoiceChannel> VoiceChannels => _client.VoiceChannels.Values.Where(x => x.ServerId == Id);
        public IEnumerable<ServerMember> Members => _client.ServerMembers[Id].Values;
        public DiscordVoiceClient VoiceClient { get; internal set; }
        public string MyVoiceSessionId { get; internal set; }

        DiscordClient _client;

        ulong? _afkChannelId;

        public Server(DiscordGuild discordGuild, DiscordClient client)
        {
            Id = discordGuild.Id;
            _client = client;
            _afkChannelId = discordGuild.AfkChannelId;
            AfkTimeout = discordGuild.AfkTimeout;
            // discordGuild.DefaultMessageNotificationsLevel
            // discordGuild.EmbeddedChannelId
            // discordGuild.Emojis
            // discordGuild.Features
            // discordGuild.IconHash
            // discordGuild.IsGuildEmbeddable
            // discordGuild.IsLarge
            // discordGuild.IsUnavailable
            // discordGuild.JoinedDate
            // discordGuild.MemberCount
            // discordGuild.MultiFactorAuthenticationLevel
            // discordGuild.Name
            // discordGuild.OwnerId
            // discordGuild.Presences
            // discordGuild.Roles
            // discordGuild.SplashHash
            // discordGuild.VerificationLevel
            // discordGuild.VoiceRegionId
            // discordGuild.VoiceStates
        }

        public async Task LeaveVoice()
        {
            await _client.LeaveVoiceChannelInServer(this);
        }

        public override string ToString()
        {
            var str = "{ ";

            str += nameof(Id) + ": " + Id;
            str += ", ";
            str += nameof(TextChannels) + ": " + TextChannels.Count();
            str += ", ";
            str += nameof(AfkChannel) + ": " + (AfkChannel != null ? AfkChannel.Name : "null");
            str += ", ";
            str += nameof(VoiceChannels) + ": " + VoiceChannels.Count();
            str += ", ";
            str += nameof(Members) + ": " + Members.Count();
            str += ", ";
            str += nameof(VoiceClient) + ": " + (VoiceClient != null ? "exists" : "null");
            str += ", ";
            str += nameof(MyVoiceSessionId) + ": " + (MyVoiceSessionId != null ? MyVoiceSessionId : "null");
            str += ", ";
            str += nameof(AfkTimeout) + ": " + AfkTimeout;
            str += " }";

            return str;
        }
    }
}