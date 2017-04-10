using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Extensions;
using DiscordApiWrapper.Models;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        public event Action<string> HelloReceived;

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient), ConsoleColor.DarkGreen);

        readonly WebSocketClient _clientWebSocketWrapper;

        int _lastSequenceReceived;
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
        }

        async void OnHelloReceivedAsync(string eventJson)
        {
            _logger.LogInfo("Received Hello from Voice Server", ConsoleColor.Green);
            var hello = JsonConvert.DeserializeObject<VoiceServerHello>(eventJson.ToString());

            await SendIdentifyAsync();
            StartHeartBeatLoop(hello.HeartbeatInterval);
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
            await SendOpCodeAsync(VoiceOpCode.Heartbeat, _lastSequenceReceived);
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
            var jsonGatewayPayload = gatewayPayload.Serialize();

            _logger.LogDebug($"Sending opcode {gatewayPayload.VoiceOpCode} to gateway...");
            _logger.LogTrace("" + jsonGatewayPayload);

            await _clientWebSocketWrapper.SendMessageUsingQueueAsync(jsonGatewayPayload);

            _logger.LogDebug($"Sent {gatewayPayload.VoiceOpCode}");
        }
        #endregion

        public async Task ConnectAsync()
        {
            await _clientWebSocketWrapper.ConnectAsync();
            _logger.LogInfo($"Connected to VoiceServer", ConsoleColor.Green);
        }
        
        void OnMessageReceived()
        {
            string message = _clientWebSocketWrapper.ReceivedMessages.Dequeue();
            var payload = JsonConvert.DeserializeObject<VoiceServerPayload>(message);

            StoreSequenceNumberForHeartbeat(payload);

            LogMessageReceived(message, payload);

            switch (payload.VoiceOpCode)
            {
                case VoiceOpCode.Hello: InvokeEvent(HelloReceived, payload); break;
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

        void StoreSequenceNumberForHeartbeat(VoiceServerPayload payload)
        {
            if (payload.SequenceNumber.HasValue)
            {
                _lastSequenceReceived = payload.SequenceNumber.Value;
            }
        }

        void InvokeEvent(Action<string> handler, VoiceServerPayload payload)
        {
            handler?.Invoke(payload.EventData?.ToString());
        }
    }
}