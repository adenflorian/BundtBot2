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

namespace BundtCord.Discord
{
    public class DiscordClient
    {
        public const string Name = "bundtbot";
        const string Version = "0.0.2";

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordClient));

        public IUser Me { get; internal set; }

        public delegate void MessageCreatedHandler(IMessage message);
        public event MessageCreatedHandler MessageCreated;
        public delegate void ServerCreatedHandler(IServer server);
        public event ServerCreatedHandler ServerCreated;

        internal IDiscordRestClient DiscordRestClient;

        // This data must be cleared when Invalid Session is received from gateway
        internal Dictionary<ulong, ITextChannel> TextChannels = new Dictionary<ulong, ITextChannel>();
        internal Dictionary<ulong, VoiceChannel> VoiceChannels = new Dictionary<ulong, VoiceChannel>();
        internal Dictionary<ulong, IUser> Users = new Dictionary<ulong, IUser>();

        readonly string _botToken;
        
        DiscordGatewayClient _gatewayClient;
        VoiceServerInfo _latestVoiceServerInfo;
        string _sessionId;

        public DiscordClient(string botToken)
        {
            _botToken = botToken;
            var config = new RestClientConfig
            {
                BotToken = _botToken,
                Name = Name,
                Version = Version,
                BaseAddress = new Uri("https://discordapp.com/api/")
            };
            DiscordRestClient = new DiscordRestClientProxy(config);
        }

        public async Task ConnectAsync()
        {
            var gatewayUrl = await DiscordRestClient.GetGatewayUrlAsync();

            _gatewayClient = new DiscordGatewayClient(_botToken, gatewayUrl);

            await _gatewayClient.ConnectAsync();

            _gatewayClient.GuildCreated += (discordGuild) =>
            {
                var newServer = new Server(discordGuild, this);

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
                    .ToList().ForEach(x => { Users[x.Id] = x; });
                
                discordGuild.VoiceStates.ForEach(x => ProcessVoiceState(x));

                ServerCreated?.Invoke(newServer);
            };

            _gatewayClient.InvalidSessionReceived += async (string eventName, string eventJsonData) =>
            {
                _logger.LogInfo("Received InvalidSession from Gateway, clearing state data...", ConsoleColor.Red);
                TextChannels.Clear();
                VoiceChannels.Clear();
                Users.Clear();
                Me = null;

                await _gatewayClient.SendIdentifyAsync();
            };

            _gatewayClient.MessageCreated += (discordMessage) =>
            {
                var message = new Message(discordMessage, this);
                MessageCreated?.Invoke(message);
            };

            _gatewayClient.Ready += (readyInfo) =>
            {
                Me = new User(readyInfo.User, this);
                _sessionId = readyInfo.SessionId;
            };

            _gatewayClient.VoiceStateUpdate += (voiceState) =>
            {
                ProcessVoiceState(voiceState);
            };
            
            _gatewayClient.VoiceServerUpdate += (voiceServerInfo) =>
            {
                _latestVoiceServerInfo = voiceServerInfo;
            };
        }

        void ProcessVoiceState(VoiceState voiceState)
        {
            ((User)Users[voiceState.UserId]).VoiceChannel = voiceState.ChannelId.HasValue ? VoiceChannels[voiceState.ChannelId.Value] : null;
        }

        public async void SetGameAsync(string gameName)
        {
            await _gatewayClient.SendStatusUpdateAsync(new StatusUpdate(null, gameName));
        }

        public async Task JoinVoiceChannel(VoiceChannel voiceChannel, bool muted = false, bool deafened = false)
        {
            await _gatewayClient.SendVoiceStateUpdateAsync(new GatewayVoiceStateUpdate
            {
                GuildId = voiceChannel.ServerId,
                VoiceChannelId = voiceChannel.Id,
                IsMutedBySelf = muted,
                IsDeafenedBySelf = deafened
            });

            // Wait for voice server info
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
