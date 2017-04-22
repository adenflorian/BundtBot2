using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Gateway;
using BundtCommon;
using DiscordApiWrapper.Gateway.Models;
using DiscordApiWrapper.Models;
using DiscordApiWrapper.RestApi;
using DiscordApiWrapper.Voice;

namespace BundtCord.Discord
{
    public class DiscordClient
    {
        public const string LibraryName = "libundtbot";
        public const string Version = "0.0.4";

        public User Me { get; internal set; }

        public event Action<TextChannelMessage> TextChannelMessageReceived;
        public event Action<Server> ServerCreated;

        internal IDiscordRestClient DiscordRestClient;

        // All of these dictionaries must be cleared when Invalid Session is received from gateway
        internal Dictionary<ulong, Server> Servers = new Dictionary<ulong, Server>();
        internal Dictionary<ulong, Dictionary<ulong, ServerMember>> ServerMembers = new Dictionary<ulong, Dictionary<ulong, ServerMember>>();
        internal Dictionary<ulong, TextChannel> TextChannels = new Dictionary<ulong, TextChannel>();
        internal Dictionary<ulong, VoiceChannel> VoiceChannels = new Dictionary<ulong, VoiceChannel>();
        internal Dictionary<ulong, User> Users = new Dictionary<ulong, User>();

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

        readonly string _botToken;

        DiscordGatewayClient _gatewayClient;
        VoiceServerInfo _latestVoiceServerInfo;
        string _sessionId;

        public DiscordClient(string botToken)
        {
            _botToken = botToken;
            var config = new RestClientConfig(_botToken, LibraryName, Version, new Uri("https://discordapp.com/api/"));
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
            _gatewayClient.GuildCreated += OnGuildCreated;
            _gatewayClient.InvalidSessionReceived += OnInvalidSessionReceivedAsync;
            _gatewayClient.MessageCreated += OnMessageCreated;
            _gatewayClient.Ready += OnReady;
            _gatewayClient.VoiceServerUpdate += OnVoiceServerUpdateAsync;
            _gatewayClient.VoiceStateUpdate += OnVoiceStateUpdate;
        }

        void OnGuildCreated(DiscordGuild discordGuild)
        {
            var newServer = new Server(discordGuild, this);
            Servers.Add(newServer.Id, newServer);
            ServerMembers[newServer.Id] = new Dictionary<ulong, ServerMember>();

            discordGuild.AllChannels
                .Where(x => x.Type == GuildChannelType.Text)
                .Select(x => new TextChannel(x, this))
                .ToList()
                .ForEach(x => { TextChannels[x.Id] = x; });
            discordGuild.AllChannels
                .Where(x => x.Type == GuildChannelType.Voice)
                .Select(x => new VoiceChannel(x, this))
                .ToList()
                .ForEach(x => { VoiceChannels[x.Id] = x; });
            discordGuild.Members
                .Select(x => new User(x.User, this))
                .ToList()
                .ForEach(x => { Users[x.Id] = x; });
            discordGuild.Members
                .Select(x => new ServerMember(discordGuild.Id, x.User.Id, this) as ServerMember)
                .ToList()
                .ForEach(x => { ServerMembers[x.Server.Id][x.User.Id] = x; });

            discordGuild.VoiceStates.ForEach(x => ProcessVoiceState(x));

            ServerCreated?.Invoke(newServer);
        }

        async void OnInvalidSessionReceivedAsync(string eventName, string eventJsonData)
        {
            _logger.LogInfo("Received InvalidSession from Gateway, "
                + "clearing state data then sending Identify...", ConsoleColor.Red);

            TextChannels.Clear();
            VoiceChannels.Clear();
            Users.Clear();
            Servers.Clear();
            Me = null;

            await _gatewayClient.SendIdentifyAsync();
        }

        void OnMessageCreated(DiscordMessage discordMessage)
        {
            if (TextChannels.ContainsKey(discordMessage.ChannelId))
            {
                var serverId = TextChannels[discordMessage.ChannelId].ServerId;
                var message = new TextChannelMessage(discordMessage, serverId, this);
                TextChannelMessageReceived?.Invoke(message);
            }
            else
            {
                _logger.LogWarning("Received DM Message but not prepared for it...");
            }
        }

        void OnReady(Ready readyInfo)
        {
            Me = new User(readyInfo.User, this);
            _sessionId = readyInfo.SessionId;
        }

        async void OnVoiceServerUpdateAsync(VoiceServerInfo voiceServerInfo)
        {
            // TODO: This is bad
            _latestVoiceServerInfo = voiceServerInfo;

            var server = Servers[voiceServerInfo.GuildID];

            if (server.VoiceClient == null)
            {
                var voiceClient = new DiscordVoiceClient();
                await voiceClient.ConnectAsync(voiceServerInfo, Me.Id, _sessionId);
                server.VoiceClient = voiceClient;
            }
        }

        void OnVoiceStateUpdate(VoiceState voiceState)
        {
            ProcessVoiceState(voiceState);
        }

        void ProcessVoiceState(VoiceState voiceState)
        {
            if (voiceState.ChannelId.HasValue)
            {
                // Join or move channel

                // get server id for channel
                var serverId = VoiceChannels[voiceState.ChannelId.Value].ServerId;
                ServerMembers[serverId][voiceState.UserId].VoiceChannelId = voiceState.ChannelId;
            }
            else
            {
                // left channel
                ServerMembers[voiceState.GuildID.Value][voiceState.UserId].VoiceChannelId = voiceState.ChannelId;
            }
        }

        public async void SetGameAsync(string gameName)
        {
            await _gatewayClient.SendStatusUpdateAsync(new StatusUpdate(null, gameName));
        }

        /// <summary>
        /// Won't return until it is ok to send voice to this channel
        /// </summary>
        public async Task JoinVoiceChannel(VoiceChannel voiceChannel, bool muted = false, bool deafened = false)
        {
            await _gatewayClient.SendVoiceStateUpdateAsync(new GatewayVoiceStateUpdate
            {
                GuildId = voiceChannel.ServerId,
                VoiceChannelId = voiceChannel.Id,
                IsMutedBySelf = muted,
                IsDeafenedBySelf = deafened
            });

            await Wait.Until(() => voiceChannel.Server.VoiceClient != null && voiceChannel.Server.VoiceClient.IsReady)
                .CheckingEvery(TimeEx._100ms)
                .For(TimeEx._5seconds)
                .StartAsync();
        }

        public async Task LeaveVoiceChannelInServer(Server server, bool muted = false, bool deafened = false)
        {
            await _gatewayClient.SendVoiceStateUpdateAsync(new GatewayVoiceStateUpdate
            {
                GuildId = server.Id,
                VoiceChannelId = null,
                IsMutedBySelf = muted,
                IsDeafenedBySelf = deafened
            });

            // TODO Destroy that server's voice client properly
            server.VoiceClient.Dispose();
            server.VoiceClient = null;
        }
    }
}
