using System;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Events;
using BundtBot.Discord.Models.Gateway;
using BundtBot.Extensions;
using Newtonsoft.Json;

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
        public delegate void ReadyHandler(Ready readyInfo);
        public event ReadyHandler Ready;
        public delegate void MessageCreatedHandler(DiscordMessage discordMessage);
        public event MessageCreatedHandler MessageCreated;
        public delegate void GuildCreatedHandler(DiscordGuild discordGuild);
        /// <summary>
        /// This event can be sent in three different scenarios:
        ///   1. When a user is initially connecting, to lazily load and backfill
        ///      information for all unavailable guilds sent in the ready event.
        ///   2. When a Guild becomes available again to the client.
        ///   3. When the current user joins a new Guild.
        /// The inner payload is a guild object, with all the extra fields specified.
        /// </summary>
        public event GuildCreatedHandler GuildCreated;

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordGatewayClient), ConsoleColor.Cyan);

        readonly ClientWebSocketWrapper _clientWebSocketWrapper;
        readonly string _authToken;

        TimeSpan _heartbeatInterval;
        int _lastSequenceReceived;
        bool _readyEventHasNotBeenProcessed = true;
		string _sessionId;

        public DiscordGatewayClient(string authToken, Uri gatewayUri)
        {
            _authToken = authToken;
            var modifiedGatewayUrl = gatewayUri.AddParameter("v", "5").AddParameter("encoding", "'json'");
            _clientWebSocketWrapper = new ClientWebSocketWrapper(modifiedGatewayUrl);
            HelloReceived += OnHelloReceived;
            HeartbackAckReceived += HeartbackAckOperation.Instance.Execute;
            Ready += OnReady;
            DispatchReceived += OnDispatchReceived;
            _clientWebSocketWrapper.MessageReceived += OnMessageReceived;
        }

        public async Task ConnectAsync()
        {
            await _clientWebSocketWrapper.ConnectAsync();
            _logger.LogInfo($"Connected to Gateway", ConsoleColor.Green);
        }

        async void OnHelloReceived(string eventName, string eventData)
        {
            _logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
            var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
            _heartbeatInterval = hello.HeartbeatInterval;

            if (_readyEventHasNotBeenProcessed)
            {
                StartHeartBeatLoop();
                await SendGatewayIdentify();
            }
            else
            {
                await SendHeartBeatAsync();
                await SendGatewayResume();
            }
        }

        void StartHeartBeatLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await SendHeartBeatAsync();
                    await Task.Delay(_heartbeatInterval);
                }
            });
            _logger.LogInfo($"Heartbeat loop started with interval of {_heartbeatInterval.TotalSeconds} seconds", ConsoleColor.Green);
        }

        public async Task SendHeartBeatAsync()
        {
            _logger.LogInfo(
                new LogMessage("Sending Heartbeat "),
                new LogMessage("♥", ConsoleColor.Red),
                new LogMessage(" →"));
            await SendOpCodeAsync(OpCode.Heartbeat, _lastSequenceReceived);
        }

        public async Task SendGatewayIdentify()
        {
            _logger.LogInfo("Sending GatewayIdentify to Gateway", ConsoleColor.Green);
            await SendOpCodeAsync(OpCode.Identify, new GatewayIdentify
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
                LargeThreshold = Threshold.Maximum
            });
        }

        async Task SendGatewayResume()
        {
            _logger.LogInfo($"Sending GatewayResume to Gateway (session_id: {_sessionId})", ConsoleColor.Green);
            await SendOpCodeAsync(OpCode.Resume, new GatewayResume
            {
                SessionToken = _authToken,
                SessionId = _sessionId,
                LastSequenceNumberReceived = _lastSequenceReceived
            });
        }

        void OnReady(Ready readyInfo)
        {
            _readyEventHasNotBeenProcessed = false;
            _sessionId = readyInfo.SessionId;
        }

        public async Task SendStatusUpdate(StatusUpdate statusUpdate)
        {
            _logger.LogInfo("Sending StatusUpdate to Gateway" +
                            $"(idle since: {statusUpdate.IdleSince}, " +
                            $"game: {statusUpdate.Game.Name}",
                            ConsoleColor.Green);
            await SendOpCodeAsync(OpCode.StatusUpdate, statusUpdate);
        }

        async Task SendOpCodeAsync(OpCode opCode, object eventData)
        {
            var gatewayPayload = new GatewayPayload(opCode, eventData);
            var jsonGatewayPayload = gatewayPayload.Serialize();

            _logger.LogDebug($"Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
            _logger.LogTrace("" + jsonGatewayPayload);

            await _clientWebSocketWrapper.SendMessageUsingQueueAsync(jsonGatewayPayload);

            _logger.LogDebug($"Sent {gatewayPayload.GatewayOpCode}");
        }

        void OnMessageReceived()
        {
            string message = _clientWebSocketWrapper.ReceivedMessages.Dequeue();
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(message);

            StoreSequenceNumberForHeartbeat(payload);

            LogMessageReceived(message, payload);

            switch (payload.GatewayOpCode)
            {
                case OpCode.Dispatch: InvokeEvent(DispatchReceived, payload); break;
                case OpCode.HeartbackAck: InvokeEvent(HeartbackAckReceived, payload); break;
                case OpCode.Hello: InvokeEvent(HelloReceived, payload); break;
                //case OpCode.Heartbeat: break;
                //case OpCode.Identify: break;
                //case OpCode.StatusUpdate: break;
                //case OpCode.VoiceStateUpdate: break;
                //case OpCode.VoiceServerPing: break;
                //case OpCode.Resume: break;
                //case OpCode.Reconnect: break;
                //case OpCode.RequestGuildMembers: break;
                case OpCode.InvalidSession: InvokeEvent(InvalidSessionReceived, payload); break;
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
    }
}
