using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtBot.Extensions;
using DiscordApiWrapper.Models;
using DiscordApiWrapper.Voice.VoiceGateway;
using DiscordApiWrapper.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordApiWrapper.Voice
{
    /// <summary>
    /// Will be one per server
    /// </summary>
    class VoiceGatewayClient : IDisposable
    {
        public event Action<VoiceServerReady> ReadyReceived;
        public event Action<VoiceServerSession> SessionReceived;
        
        event Action HelloReceived;
        event Action HeartbeatAckReceived;

        readonly MyLogger _logger;

        readonly WebSocketClient _webSocketClient;

        ulong _guildId;
        string _token;
        string _sessionId;
        ulong _userId;
        Timer _heartbeatTimer;
        bool _isDisposed;

        public VoiceGatewayClient(VoiceServerInfo voiceServerInfo, ulong userId, string sessionId)
        {
            _guildId = voiceServerInfo.GuildID;
            _token = voiceServerInfo.Token;
            _userId = userId;
            _sessionId = sessionId;

            _logger = new MyLogger(nameof(VoiceGatewayClient) + "-" + voiceServerInfo.GuildID.ToString().Substring(0, 4), ConsoleColor.DarkGreen);
            _logger.SetLogLevel(BundtFig.GetValue("loglevel-voicegatewayclient"));

            var modifiedUrl = voiceServerInfo.Endpoint.AddParameter("v", "5").AddParameter("encoding", "'json'");
            _webSocketClient = new WebSocketClient(modifiedUrl, "Voice-" + voiceServerInfo.GuildID.ToString().Substring(0, 4) + "-", ConsoleColor.DarkGreen);
            _webSocketClient.MessageReceived += OnMessageReceived;

            HelloReceived += OnHelloReceivedAsync;
            ReadyReceived += OnReadyReceivedAsync;
            HeartbeatAckReceived += OnHeartbeatAckReceived;
            SessionReceived += OnSessionReceivedAsync;
        }

        public async Task ConnectAsync()
        {
            await _webSocketClient.ConnectAsync();
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
            _logger.LogInfo("Received HeartbeatAck from Voice Server and ignoring...", ConsoleColor.Green);
        }

        void OnReadyReceivedAsync(VoiceServerReady voiceServerReady)
        {
            _logger.LogInfo("Received Ready from Voice Server", ConsoleColor.Green);
            StartHeartBeatLoop(voiceServerReady.HeartbeatInterval);
        }

        void OnSessionReceivedAsync(VoiceServerSession obj)
        {
            //await SendSpeakingTrueAsync();
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

        internal async Task SendSpeakingAsync(bool isSpeaking)
        {
            _logger.LogInfo($"Sending Speaking to Voice Server (isSpeaking: {isSpeaking})", ConsoleColor.Green);
            await SendOpCodeAsync(VoiceOpCode.Speaking, new VoiceServerSpeakingClient
            {
                IsSpeaking = isSpeaking,
                Delay = 0
            });
        }

        public async Task SendSpeakingTrueAsync()
        {
            _logger.LogInfo($"Sending Speaking to Voice Server (isSpeaking: true)", ConsoleColor.Green);
            await SendOpCodeAsync(VoiceOpCode.Speaking, new VoiceServerSpeakingClient
            {
                IsSpeaking = true,
                Delay = 0
            });
        }

        public async Task SendSpeakingFalseAsync()
        {
            _logger.LogInfo($"Sending Speaking to Voice Server (isSpeaking: false)", ConsoleColor.Green);
            await SendOpCodeAsync(VoiceOpCode.Speaking, new VoiceServerSpeakingClient
            {
                IsSpeaking = false,
                Delay = 0
            });
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

        public async Task SendSelectProtocolAsync(string clientPublicIpAddress, int clientPort, string encryptionMethod)
        {
            _logger.LogInfo("Sending SelectProtocol to Voice Server", ConsoleColor.Green);
            await SendOpCodeAsync(VoiceOpCode.Select, new VoiceSelectProtocol
            {
                Protocol = "udp",
                Data = new VoiceSelectProtocolData
                {
                    ClientPublicIpAddress = clientPublicIpAddress,
                    ClientPort = clientPort,
                    EncryptionMethod = encryptionMethod
                }
            });
        }

        async Task SendOpCodeAsync(VoiceOpCode opCode, object eventData)
        {
            var gatewayPayload = new VoiceServerPayload(opCode, eventData);

            _logger.LogDebug($"Sending opcode {gatewayPayload.VoiceOpCode} to gateway...");
            _logger.LogTrace("" + JObject.FromObject(gatewayPayload));

            var jsonGatewayPayload = JsonConvert.SerializeObject(gatewayPayload);
            await _webSocketClient.SendMessageUsingQueueAsync(jsonGatewayPayload);

            _logger.LogDebug($"Sent {gatewayPayload.VoiceOpCode}");
        }
        #endregion
        
        void OnMessageReceived(string message)
        {
            var payload = message.Deserialize<VoiceServerPayload>();

            LogMessageReceived(message, payload);

            switch (payload.VoiceOpCode)
            {
                case VoiceOpCode.Hello:
                    HelloReceived?.Invoke();
                    break;
                case VoiceOpCode.HeartbeatAck:
                    HeartbeatAckReceived?.Invoke();
                    break;
                case VoiceOpCode.Ready:
                    ReadyReceived?.Invoke(payload.EventData?.ToString().Deserialize<VoiceServerReady>());
                    break;
                case VoiceOpCode.Session:
                    SessionReceived?.Invoke(payload.EventData?.ToString().Deserialize<VoiceServerSession>());
                    break;
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

        ~VoiceGatewayClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing");
                    _webSocketClient.Dispose();
                    _heartbeatTimer.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}