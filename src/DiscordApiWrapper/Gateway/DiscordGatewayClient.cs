using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Events;
using BundtBot.Discord.Models.Gateway;
using BundtBot.Extensions;
using DiscordApiWrapper.Gateway;
using DiscordApiWrapper.Gateway.Models;
using DiscordApiWrapper.Models;
using DiscordApiWrapper.Models.Events;
using DiscordApiWrapper.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BundtBot.Discord.Gateway
{
    public class DiscordGatewayClient
    {
        public delegate void OperationHandler(string eventJsonData);
        public event Action<GatewayEvent, string> DispatchReceived;
        public event OperationHandler HeartbackAckReceived;
        public event OperationHandler HelloReceived;
        /// <summary>All state info that is set in the Ready and GuildCreated events must be cleared 
        /// when an InvalidSession opcode is received. Once that is done, call SendGatewayIdentify.</summary>
        public event OperationHandler InvalidSessionReceived;

        public delegate void GatewayEventHandler<T>(T eventData);
        /// <summary>This event is sent after Identify, when a Guild becomes available again to the client, 
        /// and when the current user joins a new Guild.</summary>
        public event GatewayEventHandler<DiscordGuild> GuildCreated;
        public event GatewayEventHandler<DiscordMessage> MessageCreated;
        public event GatewayEventHandler<Ready> Ready;
        public event GatewayEventHandler<TypingStart> TypingStart;
        public event GatewayEventHandler<VoiceState> VoiceStateUpdate;
        public event GatewayEventHandler<VoiceServerInfo> VoiceServerUpdate;
        event GatewayEventHandler<Channel> ChannelCreated;
        public event GatewayEventHandler<DmChannel> DmChannelCreated;
        public event GatewayEventHandler<GuildChannel> GuildChannelCreated;
        public event GatewayEventHandler<Resumed> Resumed;    // TODO Need to test
        public event GatewayEventHandler<GuildChannel> ChannelUpdated;    // TODO Need to test
        event GatewayEventHandler<Channel> ChannelDeleted;    // TODO Need to test
        //public event GatewayEventHandler<DmChannel> DmChannelDeleted;    // TODO Need to test
        //public event GatewayEventHandler<GuildChannel> GuildChannelDeleted;    // TODO Need to test
        public event GatewayEventHandler<DiscordGuild> GuildUpdated;    // TODO Need to test
        event GatewayEventHandler<GuildDeleted> GuildDeleted;    // TODO Need to test
        public event GatewayEventHandler<GuildBanAdd> GuildBanAdded;    // TODO Need to test
        public event GatewayEventHandler<GuildBanRemove> GuildBanRemoved;    // TODO Need to test
        public event GatewayEventHandler<GuildEmojisUpdate> GuildEmojisUpdated;    // TODO Need to test
        public event GatewayEventHandler<GuildIntegrationsUpdate> GuildIntegrationsUpdated;    // TODO Need to test
        public event GatewayEventHandler<GuildMemberAdd> GuildMemberAdded;    // TODO Need to test
        public event GatewayEventHandler<GuildMemberRemove> GuildMemberRemoved;    // TODO Need to test
        public event GatewayEventHandler<GuildMemberUpdate> GuildMemberUpdated;    // TODO Need to test
        public event GatewayEventHandler<GuildMembersChunk> GuildMembersChunked;    // TODO Need to test
        public event GatewayEventHandler<GuildRoleCreate> GuildRoleCreated;    // TODO Need to test
        public event GatewayEventHandler<GuildRoleUpdate> GuildRoleUpdated;    // TODO Need to test
        public event GatewayEventHandler<GuildRoleDelete> GuildRoleDeleted;    // TODO Need to test
        public event GatewayEventHandler<DiscordMessage> MessageUpdated;    // TODO Need to test
        public event GatewayEventHandler<MessageDelete> MessageDeleted;    // TODO Need to test
        public event GatewayEventHandler<MessageDeleteBulk> MessageDeleteBulk;    // TODO Need to test
        //TODO public event GatewayEventHandler<PresenceUpdate> PresenceUpdated;    // TODO Need to test
        public event GatewayEventHandler<DiscordUser> UserUpdated;    // TODO Need to test

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordGatewayClient), ConsoleColor.Cyan);

        readonly WebSocketClient _webSocketClient;
        readonly string _authToken;

        int _lastSequenceReceived;
        bool _readyEventHasNotBeenProcessed = true;
		string _sessionId;
        Timer _heartbeatTimer;

        public DiscordGatewayClient(string authToken, Uri gatewayUri)
        {
            _authToken = authToken;
            _logger.SetLogLevel(BundtFig.GetValue("loglevel-discordgatewayclient"));

            var modifiedGatewayUrl = gatewayUri.AddParameter("v", "5").AddParameter("encoding", "'json'");

            _webSocketClient = new WebSocketClient(modifiedGatewayUrl, "Gateway-", ConsoleColor.Cyan);

            HelloReceived += OnHelloReceivedAsync;
            HeartbackAckReceived += (d) => _logger.LogInfo(new LogMessage("HeartbackAck Received ← "), new LogMessage("♥", ConsoleColor.Red));
            Ready += OnReady;
            DispatchReceived += OnDispatchReceived;
            ChannelCreated += OnChannelCreated;
            
            _webSocketClient.MessageReceived += OnMessageReceived;
        }

        public async Task ConnectAsync()
        {
            await _webSocketClient.ConnectAsync();
            _logger.LogInfo($"Connected to Gateway", ConsoleColor.Green);
        }

        #region Handlers
        async void OnHelloReceivedAsync(string eventData)
        {
            _logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
            var hello = eventData.Deserialize<GatewayHello>();

            if (_readyEventHasNotBeenProcessed)
            {
                StartHeartBeatLoop(hello.HeartbeatInterval);
                await SendIdentifyAsync();
            }
            else
            {
                StartHeartBeatLoop(hello.HeartbeatInterval);
                await SendResumeAsync();
            }
        }

        void StartHeartBeatLoop(TimeSpan heartbeatInterval)
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new Timer(async (o) => await SendHeartbeatAsync(), null, TimeSpan.Zero, heartbeatInterval);
            _logger.LogInfo($"Heartbeat loop started with interval of {heartbeatInterval.TotalSeconds} seconds", ConsoleColor.Green);
        }

        void OnReady(Ready readyInfo)
        {
            _readyEventHasNotBeenProcessed = false;
            _sessionId = readyInfo.SessionId;
        }

        void OnMessageReceived(string message)
        {
            var payload = message.Deserialize<GatewayPayload>();

            StoreSequenceNumberForHeartbeat(payload);

            LogMessageReceived(message, payload);

            switch (payload.GatewayOpCode)
            {
                case GatewayOpCode.Dispatch: DispatchReceived?.Invoke(payload.EventName.Value, payload.EventData?.ToString()); break;
                case GatewayOpCode.HeartbeatAck: InvokeEvent(HeartbackAckReceived, payload); break;
                case GatewayOpCode.Hello: InvokeEvent(HelloReceived, payload); break;
                case GatewayOpCode.InvalidSession: InvokeEvent(InvalidSessionReceived, payload); break;
                default:
                    _logger.LogWarning($"Received an OpCode with no handler: {payload.GatewayOpCode}");
                    break;
            }
        }

        void LogMessageReceived(string message, GatewayPayload payload)
        {
            _logger.LogDebug($"Message received from gateway (opcode: {payload.GatewayOpCode}, sequence: {payload.SequenceNumber})");
            _logger.LogTrace(message.Prettify());
        }

        void StoreSequenceNumberForHeartbeat(GatewayPayload receivedGatewayDispatch)
        {
            if (receivedGatewayDispatch.SequenceNumber.HasValue)
            {
                _lastSequenceReceived = receivedGatewayDispatch.SequenceNumber.Value;
            }
        }

        void InvokeEvent(OperationHandler handler, GatewayPayload payload)
        {
            handler?.Invoke(payload.EventData?.ToString());
        }

        void OnDispatchReceived(GatewayEvent eventName, string eventJsonData)
        {
            _logger.LogInfo(new LogMessage("Received Event: "), new LogMessage(eventName.ToString(), ConsoleColor.Cyan));

            switch (eventName)
            {
                case GatewayEvent.Channel_Create:
                    var newChannel = eventJsonData.Deserialize<Channel>();
                    newChannel.OriginalEventJson = eventJsonData;
                    ChannelCreated?.Invoke(newChannel);
                    break;
                case GatewayEvent.Message_Create: HandleEvent<DiscordMessage>(eventJsonData, MessageCreated); break;
                case GatewayEvent.Guild_Create: HandleEvent<DiscordGuild>(eventJsonData, GuildCreated); break;
                case GatewayEvent.Ready: HandleEvent<Ready>(eventJsonData, Ready); break;
                case GatewayEvent.Typing_Start: HandleEvent<TypingStart>(eventJsonData, TypingStart); break;
                case GatewayEvent.Voice_State_Update: HandleEvent<VoiceState>(eventJsonData, VoiceStateUpdate); break;
                case GatewayEvent.Voice_Server_Update: HandleEvent<VoiceServerInfo>(eventJsonData, VoiceServerUpdate); break;
                case GatewayEvent.Resumed: HandleEvent<Resumed>(eventJsonData, Resumed); break;
                case GatewayEvent.Channel_Update: HandleEvent<GuildChannel>(eventJsonData, ChannelUpdated); break;
                case GatewayEvent.Channel_Delete: HandleEvent<Channel>(eventJsonData, ChannelDeleted); break;
                case GatewayEvent.Guild_Update: HandleEvent<DiscordGuild>(eventJsonData, GuildUpdated); break;
                case GatewayEvent.Guild_Delete: HandleEvent<GuildDeleted>(eventJsonData, GuildDeleted); break;
                case GatewayEvent.Guild_Ban_Add: HandleEvent<GuildBanAdd>(eventJsonData, GuildBanAdded); break;
                case GatewayEvent.Guild_Ban_Remove: HandleEvent<GuildBanRemove>(eventJsonData, GuildBanRemoved); break;
                case GatewayEvent.Guild_Emojis_Update: HandleEvent<GuildEmojisUpdate>(eventJsonData, GuildEmojisUpdated); break;
                case GatewayEvent.Guild_Integrations_Update: HandleEvent<GuildIntegrationsUpdate>(eventJsonData, GuildIntegrationsUpdated); break;
                case GatewayEvent.Guild_Member_Add: HandleEvent<GuildMemberAdd>(eventJsonData, GuildMemberAdded); break;
                case GatewayEvent.Guild_Member_Remove: HandleEvent<GuildMemberRemove>(eventJsonData, GuildMemberRemoved); break;
                case GatewayEvent.Guild_Member_Update: HandleEvent<GuildMemberUpdate>(eventJsonData, GuildMemberUpdated); break;
                case GatewayEvent.Guild_Members_Chunk: HandleEvent<GuildMembersChunk>(eventJsonData, GuildMembersChunked); break;
                case GatewayEvent.Guild_Role_Create: HandleEvent<GuildRoleCreate>(eventJsonData, GuildRoleCreated); break;
                case GatewayEvent.Guild_Role_Update: HandleEvent<GuildRoleUpdate>(eventJsonData, GuildRoleUpdated); break;
                case GatewayEvent.Guild_Role_Delete: HandleEvent<GuildRoleDelete>(eventJsonData, GuildRoleDeleted); break;
                case GatewayEvent.Message_Update: HandleEvent<DiscordMessage>(eventJsonData, MessageUpdated); break;
                case GatewayEvent.Message_Delete: HandleEvent<MessageDelete>(eventJsonData, MessageDeleted); break;
                case GatewayEvent.Message_Delete_Bulk: HandleEvent<MessageDeleteBulk>(eventJsonData, MessageDeleteBulk); break;
                // TODO case GatewayEvent.Presence_Update: HandleEvent<PresenceUpdate>(eventJsonData, pres); break;
                case GatewayEvent.User_Update: HandleEvent<DiscordUser>(eventJsonData, UserUpdated); break;
                default: _logger.LogWarning($"Received an Event with no handler: {eventName}"); break;
            }
        }

        void HandleEvent<T>(string eventJsonData, GatewayEventHandler<T> handler)
        {
            handler?.Invoke(eventJsonData.Deserialize<T>());
        }

        void OnChannelCreated(Channel newChannel)
        {
            if (newChannel.IsPrivate)
            {
                HandleEvent<DmChannel>(newChannel.OriginalEventJson, DmChannelCreated);
            }
            else
            {
                HandleEvent<GuildChannel>(newChannel.OriginalEventJson, GuildChannelCreated);
            }
        }
        #endregion

        #region Senders
        public async Task SendHeartbeatAsync()
        {
            _logger.LogInfo(
                new LogMessage("Sending Heartbeat "),
                new LogMessage("♥", ConsoleColor.Red),
                new LogMessage(" →"));
            await SendOpCodeAsync(GatewayOpCode.Heartbeat, _lastSequenceReceived);
        }

        public async Task SendIdentifyAsync()
        {
            _logger.LogInfo("Sending GatewayIdentify to Gateway", ConsoleColor.Green);
            await SendOpCodeAsync(GatewayOpCode.Identify, new GatewayIdentify
            {
                AuthenticationToken = _authToken,
                ConnectionProperties = new ConnectionProperties
                {
                    OperatingSystem = "windows",
                    Browser = "bundtbot",
                    Device = "bundtbot",
                    Referrer = "",
                    ReferringDomain = "",
                },
                SupportsCompression = false,
                LargeThreshold = Threshold.Maximum,
                Shard = new int[] {0, 1}
            });
        }

        async Task SendResumeAsync()
        {
            _logger.LogInfo($"Sending GatewayResume to Gateway (session_id: {_sessionId})", ConsoleColor.Green);
            await SendOpCodeAsync(GatewayOpCode.Resume, new GatewayResume
            {
                SessionToken = _authToken,
                SessionId = _sessionId,
                LastSequenceNumberReceived = _lastSequenceReceived
            });
        }

        public async Task SendStatusUpdateAsync(StatusUpdate statusUpdate)
        {
            _logger.LogInfo("Sending StatusUpdate to Gateway " +
                            $"(idle since: {statusUpdate.IdleSince}, " +
                            $"game: {statusUpdate.Game.Name})",
                            ConsoleColor.Green);
            await SendOpCodeAsync(GatewayOpCode.StatusUpdate, statusUpdate);
        }

        public async Task SendVoiceStateUpdateAsync(GatewayVoiceStateUpdate gatewayVoiceStateUpdate)
        {
            _logger.LogInfo("Sending VoiceStateUpdate to Gateway " +
                            $"(guild: {gatewayVoiceStateUpdate.GuildId}, " +
                            $"channel: {gatewayVoiceStateUpdate.VoiceChannelId})",
                            ConsoleColor.Green);
            await SendOpCodeAsync(GatewayOpCode.VoiceStateUpdate, gatewayVoiceStateUpdate);
        }

        async Task SendOpCodeAsync(GatewayOpCode opCode, object eventData)
        {
            try
            {
                var gatewayPayload = new GatewayPayload(opCode, eventData);

                _logger.LogDebug($"Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
                _logger.LogTrace("" + JObject.FromObject(gatewayPayload));

                var jsonGatewayPayload = JsonConvert.SerializeObject(gatewayPayload);
                await _webSocketClient.SendMessageUsingQueueAsync(jsonGatewayPayload);

                _logger.LogDebug($"Sent {gatewayPayload.GatewayOpCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                throw;
            }
        }
        #endregion

        // TODO Implement these gateway client requests:
        //case OpCode.VoiceServerPing: break;
        //case OpCode.RequestGuildMembers: break;
    }
}
