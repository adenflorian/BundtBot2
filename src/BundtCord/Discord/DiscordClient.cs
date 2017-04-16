using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Gateway;
using DiscordApiWrapper.Gateway.Models;
using DiscordApiWrapper.Models;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.Voice;

namespace BundtCord.Discord
{
    public class DiscordClient
    {
        public const string Name = "bundtbot";
        public const string Version = "0.0.2";

        public IUser Me { get; internal set; }

        public event Action<ITextChannelMessage> TextChannelMessageReceived;
        public event Action<IServer> ServerCreated;

        internal IDiscordRestClient DiscordRestClient;

        // This data must be cleared when Invalid Session is received from gateway
        internal Dictionary<ulong, IServer> Servers = new Dictionary<ulong, IServer>();
        internal Dictionary<ulong, Dictionary<ulong, IServerMember>> ServerMembers = new Dictionary<ulong, Dictionary<ulong, IServerMember>>();
        internal Dictionary<ulong, ITextChannel> TextChannels = new Dictionary<ulong, ITextChannel>();
        internal Dictionary<ulong, IVoiceChannel> VoiceChannels = new Dictionary<ulong, IVoiceChannel>();
        internal Dictionary<ulong, IUser> Users = new Dictionary<ulong, IUser>();

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

        readonly string _botToken;

        DiscordGatewayClient _gatewayClient;
        VoiceServerInfo _latestVoiceServerInfo;
        string _sessionId;

        public DiscordClient(string botToken)
        {
            _botToken = botToken;
            var config = new RestClientConfig(_botToken, Name, Version, new Uri("https://discordapp.com/api/"));
            DiscordRestClient = new DiscordRestClientProxy(config);
        }

        public async Task ConnectAsync()
        {
            var gatewayUrl = await DiscordRestClient.GetGatewayUrlAsync();

            _gatewayClient = new DiscordGatewayClient(_botToken, gatewayUrl);

            RegisterGatewayEventHandlers();

            await _gatewayClient.ConnectAsync();
        }

        void RegisterGatewayEventHandlers()
        {
            _gatewayClient.GuildCreated += (discordGuild) =>
            {
                var newServer = new Server(discordGuild, this);
                Servers.Add(newServer.Id, newServer);
                ServerMembers[newServer.Id] = new Dictionary<ulong, IServerMember>();

                discordGuild.AllChannels
                    .Where(x => x.Type == GuildChannelType.Text)
                    .Select(x => new TextChannel(x, this))
                    .ToList().ForEach(x => { TextChannels[x.Id] = x; });
                discordGuild.AllChannels
                    .Where(x => x.Type == GuildChannelType.Voice)
                    .Select(x => new VoiceChannel(x, this))
                    .ToList().ForEach(x => { VoiceChannels[x.Id] = x; });
                discordGuild.Members
                    .Select(x => new User(x.User, this))
                    .ToList()
                    .ForEach(x => { Users[x.Id] = x; });
                discordGuild.Members
                    .Select(x => new ServerMember(discordGuild.Id, x.User.Id, this) as IServerMember)
                    .ToList()
                    .ForEach(x => { ServerMembers[x.Server.Id][x.User.Id] = x; });

                discordGuild.VoiceStates.ForEach(x => ProcessVoiceState(x));

                ServerCreated?.Invoke(newServer);
            };

            _gatewayClient.InvalidSessionReceived += async (string eventName, string eventJsonData) =>
            {
                _logger.LogInfo("Received InvalidSession from Gateway, "
                    + "clearing state data then sending Identify...", ConsoleColor.Red);

                TextChannels.Clear();
                VoiceChannels.Clear();
                Users.Clear();
                Servers.Clear();
                Me = null;

                await _gatewayClient.SendIdentifyAsync();
            };

            _gatewayClient.MessageCreated += (discordMessage) =>
            {
                if (TextChannels.ContainsKey(discordMessage.ChannelId))
                {
                    var serverId = TextChannels[discordMessage.ChannelId].ServerId;
                    var message = new TextChannelMessage(discordMessage, serverId, this);
                    TextChannelMessageReceived?.Invoke(message);
                }
                else
                {
                    // DM message
                    _logger.LogWarning("Received DM Message but not prepared for it...");
                }
            };

            _gatewayClient.Ready += (readyInfo) =>
            {
                Me = new User(readyInfo.User, this);
                _sessionId = readyInfo.SessionId;
            };

            _gatewayClient.VoiceServerUpdate += async (voiceServerInfo) =>
            {
                _latestVoiceServerInfo = voiceServerInfo;
                var voiceClient = new DiscordVoiceClient();
                await voiceClient.ConnectAsync(voiceServerInfo, Me.Id, _sessionId);
                ((Server)Servers[voiceServerInfo.GuildID]).VoiceClient = voiceClient;
            };

            _gatewayClient.VoiceStateUpdate += (voiceState) =>
            {
                ProcessVoiceState(voiceState);
            };
        }

        void ProcessVoiceState(VoiceState voiceState)
        {
            if (voiceState.ChannelId.HasValue)
            {
                // Join or move channel

                // get server id for channel
                var serverId = VoiceChannels[voiceState.ChannelId.Value].ServerId;
                ((ServerMember)ServerMembers[serverId][voiceState.UserId]).VoiceChannelId = voiceState.ChannelId;
            }
            else
            {
                // left channel
                ((ServerMember)ServerMembers[voiceState.GuildID.Value][voiceState.UserId]).VoiceChannelId = voiceState.ChannelId;
            }
        }

        public async void SetGameAsync(string gameName)
        {
            await _gatewayClient.SendStatusUpdateAsync(new StatusUpdate(null, gameName));
        }

        public async Task JoinVoiceChannel(IVoiceChannel voiceChannel, bool muted = false, bool deafened = false)
        {
            await _gatewayClient.SendVoiceStateUpdateAsync(new GatewayVoiceStateUpdate
            {
                GuildId = voiceChannel.ServerId,
                VoiceChannelId = voiceChannel.Id,
                IsMutedBySelf = muted,
                IsDeafenedBySelf = deafened
            });
        }

        public async Task LeaveVoiceChannelInServer(ulong serverId, bool muted = false, bool deafened = false)
        {
            await _gatewayClient.SendVoiceStateUpdateAsync(new GatewayVoiceStateUpdate
            {
                GuildId = serverId,
                VoiceChannelId = null,
                IsMutedBySelf = muted,
                IsDeafenedBySelf = deafened
            });
        }
    }
}
