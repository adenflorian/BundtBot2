using System;
using System.Linq;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Models;
using DiscordApiWrapper.Voice.VoiceGateway;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient : IDisposable
    {
        public bool IsReady;

        const string _desiredEncryptionMethod = "xsalsa20_poly1305";

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient));
        
        VoiceGatewayClient _voiceGatewayClient;
        VoiceUdpClient _voiceUdpClient;
        VoiceServerInfo _voiceServerInfo;

        uint _ssrcId;
        bool _isDisposed;

        public async Task ConnectAsync(VoiceServerInfo voiceServerInfo, ulong userId, string sessionId)
        {
            _voiceServerInfo = voiceServerInfo;

            _voiceGatewayClient = new VoiceGatewayClient(voiceServerInfo, userId, sessionId);

            _voiceGatewayClient.ReadyReceived += OnReadyReceivedAsync;
            _voiceGatewayClient.SessionReceived += OnSessionReceivedAsync;

            await _voiceGatewayClient.ConnectAsync();
        }

        async void OnReadyReceivedAsync(VoiceServerReady voiceServerReady)
        {
            _logger.LogInfo("Received Ready from Voice Server", ConsoleColor.Green);

            if (voiceServerReady.Modes.ToList().Contains(_desiredEncryptionMethod) == false)
            {
                _logger.LogCritical($"Ready payload does not contain {_desiredEncryptionMethod} as voice encryption mode!");
            }
            _ssrcId = voiceServerReady.SynchronizationSourceId;
            _voiceUdpClient = new VoiceUdpClient(_voiceServerInfo.Endpoint, voiceServerReady.Port, voiceServerReady.SynchronizationSourceId);
            var ipDiscoveryResult = await _voiceUdpClient.SendIpDiscoveryPacketAsync();

            await _voiceGatewayClient.SendSelectProtocolAsync(ipDiscoveryResult.IpAddress, ipDiscoveryResult.Port, _desiredEncryptionMethod);
        }

        void OnSessionReceivedAsync(VoiceServerSession voiceServerSession)
        {
            _logger.LogInfo("Received Session from Voice Server", ConsoleColor.Green);
            _voiceUdpClient.SecretKey = voiceServerSession.SecretKey;
            IsReady = true;
        }

        // TODO Handle this being called while already sending audio data
        public async Task SendAudioAsync(byte[] sodaBytes)
        {
            await _voiceGatewayClient.SendSpeakingAsync(true, _ssrcId);
            await _voiceUdpClient.SendAudioAsync(sodaBytes);
            await _voiceGatewayClient.SendSpeakingAsync(false, _ssrcId);
        }

        ~DiscordVoiceClient()
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
                    _voiceGatewayClient.Dispose();
                    _voiceUdpClient.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}