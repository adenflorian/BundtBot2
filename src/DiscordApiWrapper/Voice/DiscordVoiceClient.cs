using System;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Models;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient));
        
        DiscordVoiceGatewayClient _voiceGatewayClient;
        VoiceUdpClient _voiceUdpClient;
        VoiceServerInfo _voiceServerInfo;

        public async Task ConnectAsync(VoiceServerInfo voiceServerInfo, ulong userId, string sessionId)
        {
            _voiceServerInfo = voiceServerInfo;

            _voiceGatewayClient = new DiscordVoiceGatewayClient(voiceServerInfo, userId, sessionId);

            _voiceGatewayClient.ReadyReceived += OnReadyReceivedAsync;

            await _voiceGatewayClient.ConnectAsync();
        }

        void OnReadyReceivedAsync(VoiceServerReady voiceServerReady)
        {
            _logger.LogInfo("Received Ready from Voice Server", ConsoleColor.Green);

            //_voiceUdpClient = new VoiceUdpClient(_voiceServerInfo.Endpoint, voiceServerReady.Port);
        }
    }
}