using System;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Models;

namespace DiscordApiWrapper.Voice
{
    public class DiscordVoiceClient
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DiscordVoiceClient));

        readonly ClientWebSocketWrapper _clientWebSocketWrapper;

        public DiscordVoiceClient(VoiceServerInfo voiceServerInfo)
        {
            _clientWebSocketWrapper = new ClientWebSocketWrapper(new Uri(voiceServerInfo.Endpoint));
        }

        public async Task ConnectAsync(VoiceServerInfo voiceServerInfo)
        {
            await _clientWebSocketWrapper.ConnectAsync();
            _logger.LogInfo($"Connected to VoiceServer", ConsoleColor.Green);
        }
    }
}