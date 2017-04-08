using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Discord.Gateway;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Gateway;
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

        public async Task Connect()
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
                    .Select(x => new VoiceChannel(x))
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

                await _gatewayClient.SendGatewayIdentify();
            };

            _gatewayClient.MessageCreated += (discordMessage) =>
            {
                var message = new Message(discordMessage, this);
                MessageCreated?.Invoke(message);
            };

            _gatewayClient.Ready += (readyInfo) =>
            {
                Me = new User(readyInfo.User, this);
            };

            _gatewayClient.VoiceStateUpdate += (voiceState) =>
            {
                ProcessVoiceState(voiceState);
            };
        }

        void ProcessVoiceState(VoiceState voiceState)
        {
            ((User)Users[voiceState.UserId]).VoiceChannel = voiceState.ChannelId.HasValue ? VoiceChannels[voiceState.ChannelId.Value] : null;
        }

        public async void SetGame(string gameName)
        {
            await _gatewayClient.SendStatusUpdate(new StatusUpdate(null, gameName));
        }
    }
}
