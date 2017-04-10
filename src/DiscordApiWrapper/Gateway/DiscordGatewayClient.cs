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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BundtBot.Discord.Gateway
{
    public class DiscordGatewayClient
    {
        public delegate void OperationHandler(string eventName, string eventJsonData);
        public event OperationHandler DispatchReceived;
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

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordGatewayClient), ConsoleColor.Cyan);

        readonly WebSocketClient _clientWebSocketWrapper;
        readonly string _authToken;

        int _lastSequenceReceived;
        bool _readyEventHasNotBeenProcessed = true;
		string _sessionId;
        Timer _heartbeatTimer;

        public DiscordGatewayClient(string authToken, Uri gatewayUri)
        {
            _authToken = authToken;

            var modifiedGatewayUrl = gatewayUri.AddParameter("v", "5").AddParameter("encoding", "'json'");

            _clientWebSocketWrapper = new WebSocketClient(modifiedGatewayUrl, "Gateway-", ConsoleColor.Cyan);

            HelloReceived += OnHelloReceivedAsync;
            HeartbackAckReceived += (e, d) => _logger.LogInfo(new LogMessage("HeartbackAck Received ← "), new LogMessage("♥", ConsoleColor.Red));
            Ready += OnReady;
            DispatchReceived += OnDispatchReceived;
            
            _clientWebSocketWrapper.MessageReceived += OnMessageReceived;
        }

        public async Task ConnectAsync()
        {
            await _clientWebSocketWrapper.ConnectAsync();
            _logger.LogInfo($"Connected to Gateway", ConsoleColor.Green);
        }

        #region Handlers
        async void OnHelloReceivedAsync(string eventName, string eventData)
        {
            _logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
            var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());

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

        void OnMessageReceived()
        {
            string message = _clientWebSocketWrapper.ReceivedMessages.Dequeue();
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(message);

            StoreSequenceNumberForHeartbeat(payload);

            LogMessageReceived(message, payload);

            switch (payload.GatewayOpCode)
            {
                case GatewayOpCode.Dispatch: InvokeEvent(DispatchReceived, payload); break;
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
            handler?.Invoke(payload.EventName, payload.EventData?.ToString());
        }

        void OnDispatchReceived(string eventName, string eventJsonData)
        {
            _logger.LogDebug("Processing Gateway Event " + eventName);

            switch (eventName)
            {
                case "CHANNEL_CREATE":
                    var channel = JsonConvert.DeserializeObject<Channel>(eventJsonData);
                    LogReceivedEvent("CHANNEL_CREATE", channel.Id.ToString());
                    break;
                case "MESSAGE_CREATE":
                    var discordMessage = JsonConvert.DeserializeObject<DiscordMessage>(eventJsonData);
                    LogReceivedEvent("MESSAGE_CREATE", discordMessage.Content);
                    MessageCreated?.Invoke(discordMessage);
                    break;
                case "GUILD_CREATE":
                    var discordGuild = JsonConvert.DeserializeObject<DiscordGuild>(eventJsonData);
                    discordGuild.AllChannels.ForEach(x => x.GuildID = discordGuild.Id);
                    LogReceivedEvent("GUILD_CREATE", discordGuild.Name);
                    GuildCreated?.Invoke(discordGuild);
                    break;
                case "READY":
                    var ready = JsonConvert.DeserializeObject<Ready>(eventJsonData);
                    LogReceivedEvent("READY", "Our username is " + ready.User.Username);
                    Ready?.Invoke(ready);
                    break;
                case "TYPING_START":
                    var typingStart = JsonConvert.DeserializeObject<TypingStart>(eventJsonData);
                    LogReceivedEvent("TYPING_START", typingStart.UserId.ToString());
                    TypingStart?.Invoke(typingStart);
                    break;
                case "VOICE_STATE_UPDATE":
                    var voiceStateUpdate = JsonConvert.DeserializeObject<VoiceState>(eventJsonData);
                    LogReceivedEvent("VOICE_STATE_UPDATE", voiceStateUpdate.UserId.ToString());
                    VoiceStateUpdate?.Invoke(voiceStateUpdate);
                    break;
                case "VOICE_SERVER_UPDATE":
                    var voiceServerUpdate = JsonConvert.DeserializeObject<VoiceServerInfo>(eventJsonData);
                    LogReceivedEvent("VOICE_SERVER_UPDATE", voiceServerUpdate.Endpoint.ToString());
                    VoiceServerUpdate?.Invoke(voiceServerUpdate);
                    break;
                default:
                    _logger.LogWarning($"Received an Event with no handler: {eventName}");
                    break;
            }
        }

        void LogReceivedEvent(string eventName, string eventDataSummary)
        {
            _logger.LogInfo(
                new LogMessage("Received Event: "),
                new LogMessage(eventName + " ", ConsoleColor.Cyan),
                new LogMessage(eventDataSummary, ConsoleColor.DarkCyan));
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
                await _clientWebSocketWrapper.SendMessageUsingQueueAsync(jsonGatewayPayload);

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
