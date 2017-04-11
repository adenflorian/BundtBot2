using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Extensions;
using DiscordApiWrapper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        public event Action HelloReceived;
        public event Action HeartbeatAckReceived;
        public event Action<string> ReadyReceived;

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient), ConsoleColor.DarkGreen);

        readonly WebSocketClient _clientWebSocketWrapper;

        ulong _guildId;
        string _token;
        string _sessionId;
        ulong _userId;
        Timer _heartbeatTimer;

        public DiscordVoiceClient(VoiceServerInfo voiceServerInfo, ulong userId, string sessionId)
        {
            _guildId = voiceServerInfo.GuildID;
            _token = voiceServerInfo.Token;
            _userId = userId;
            _sessionId = sessionId;

            var modifiedUrl = voiceServerInfo.Endpoint.AddParameter("v", "5").AddParameter("encoding", "'json'");
            _clientWebSocketWrapper = new WebSocketClient(modifiedUrl, "Voice-", ConsoleColor.DarkGreen);
            _clientWebSocketWrapper.MessageReceived += OnMessageReceived;

            HelloReceived += OnHelloReceivedAsync;
            ReadyReceived += OnReadyReceivedAsync;
            HeartbeatAckReceived += OnHeartbeatAckReceived;
        }

        public async Task ConnectAsync()
        {
            await _clientWebSocketWrapper.ConnectAsync();
            _logger.LogInfo($"Connected to VoiceServer", ConsoleColor.Green);

            await SendIdentifyAsync();
        }

        /// <summary>
        /// Discord devs said to ignore this opcode
        /// </summary>
        /// <param name="eventJson"></param>
        void OnHelloReceivedAsync()
        {
            _logger.LogInfo("Received Hello from Voice Server and ignoring...", ConsoleColor.Green);
        }

        /// <summary>
        /// Discord devs said to ignore this opcode
        /// </summary>
        /// <param name="eventJson"></param>
        void OnHeartbeatAckReceived()
        {
            _logger.LogInfo("Received Hello from Voice Server and ignoring...", ConsoleColor.Green);
        }

        void OnReadyReceivedAsync(string eventJson)
        {
            _logger.LogInfo("Received Ready from Voice Server", ConsoleColor.Green);
            var ready = JsonConvert.DeserializeObject<VoiceServerReady>(eventJson.ToString());

            StartHeartBeatLoop(ready.HeartbeatInterval);
        }

        void StartHeartBeatLoop(TimeSpan heartbeatInterval)
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new Timer(async (o) => await SendHeartbeatAsync(), null, TimeSpan.Zero, heartbeatInterval);
            _logger.LogInfo($"Heartbeat loop started with interval of {heartbeatInterval.TotalSeconds} seconds", ConsoleColor.Green);
        }

        #region Senders
        async Task SendHeartbeatAsync()
        {
            _logger.LogInfo(
                new LogMessage("Sending Heartbeat "),
                new LogMessage("♥", ConsoleColor.Red),
                new LogMessage(" →"));
            await SendOpCodeAsync(VoiceOpCode.Heartbeat, null);
        }

        async Task SendIdentifyAsync()
        {
            _logger.LogInfo("Sending VoiceIdentify to Voice Server", ConsoleColor.Green);
            await SendOpCodeAsync(VoiceOpCode.Identify, new VoiceServerIdentify
            {
                GuildId = _guildId,
                VoiceServerToken = _token,
                UserId = _userId,
                SessionId = _sessionId
            });
        }

        async Task SendOpCodeAsync(VoiceOpCode opCode, object eventData)
        {
            var gatewayPayload = new VoiceServerPayload(opCode, eventData);

            _logger.LogDebug($"Sending opcode {gatewayPayload.VoiceOpCode} to gateway...");
            _logger.LogTrace("" + JObject.FromObject(gatewayPayload));

            var jsonGatewayPayload = JsonConvert.SerializeObject(gatewayPayload);
            await _clientWebSocketWrapper.SendMessageUsingQueueAsync(jsonGatewayPayload);

            _logger.LogDebug($"Sent {gatewayPayload.VoiceOpCode}");
        }
        #endregion
        
        void OnMessageReceived()
        {
            string message = _clientWebSocketWrapper.ReceivedMessages.Dequeue();
            var payload = JsonConvert.DeserializeObject<VoiceServerPayload>(message);

            LogMessageReceived(message, payload);

            switch (payload.VoiceOpCode)
            {
                case VoiceOpCode.Hello: HelloReceived?.Invoke(); break;
                case VoiceOpCode.HeartbeatAck: HeartbeatAckReceived?.Invoke(); break;
                case VoiceOpCode.Ready: ReadyReceived?.Invoke(payload.EventData?.ToString()); break;
                default:
                    _logger.LogWarning($"Received an OpCode with no handler: {payload.VoiceOpCode}");
                    break;
            }
        }
        
        void LogMessageReceived(string message, VoiceServerPayload payload)
        {
            _logger.LogDebug($"Message received from gateway (opcode: {payload.VoiceOpCode})");
            _logger.LogTrace(message.Prettify());
        }
    }
}