using System;
using System.Linq;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Models;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        const string _desiredEncryptionMethod = "xsalsa20_poly1305";

        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient));
        
        VoiceGatewayClient _voiceGatewayClient;
        VoiceUdpClient _voiceUdpClient;
        VoiceServerInfo _voiceServerInfo;

        public async Task ConnectAsync(VoiceServerInfo voiceServerInfo, ulong userId, string sessionId)
        {
            _voiceServerInfo = voiceServerInfo;

            _voiceGatewayClient = new VoiceGatewayClient(voiceServerInfo, userId, sessionId);

            _voiceGatewayClient.ReadyReceived += OnReadyReceivedAsync;

            await _voiceGatewayClient.ConnectAsync();
        }

        async void OnReadyReceivedAsync(VoiceServerReady voiceServerReady)
        {
            _logger.LogInfo("Received Ready from Voice Server", ConsoleColor.Green);

            if (voiceServerReady.Modes.ToList().Contains(_desiredEncryptionMethod) == false)
            {
                _logger.LogCritical($"Ready payload does not contain {_desiredEncryptionMethod} as voice encryption mode!");
            }

            _voiceUdpClient = new VoiceUdpClient(_voiceServerInfo.Endpoint, voiceServerReady.Port, voiceServerReady.SynchronizationSourceId);
            var result = await _voiceUdpClient.SendIpDiscoveryPacketAsync();
            
            await _voiceGatewayClient.SendSelectProtocolAsync(result.Item1, result.Item2, _desiredEncryptionMethod);
        }
    }
}