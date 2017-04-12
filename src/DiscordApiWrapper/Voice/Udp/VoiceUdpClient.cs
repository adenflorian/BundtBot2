using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Voice.Udp;

namespace DiscordApiWrapper.Voice
{
    class VoiceUdpClient
    {
        static readonly MyLogger _logger = new MyLogger(nameof(VoiceUdpClient), ConsoleColor.DarkGreen);

        UdpClient _udpClient;
        IPEndPoint _voiceUdpEndpoint;
        uint _ssrcId;

        public VoiceUdpClient(Uri remoteUri, int remotePort, uint ssrcId)
        {
            _ssrcId = ssrcId;

            var task = Dns.GetHostAddressesAsync(remoteUri.Host);
            task.Wait();
            Debug.Assert(task.Result.Length > 0);
            var voiceUdpServerAddress = task.Result[0];

            _udpClient = new UdpClient();
            _voiceUdpEndpoint = new IPEndPoint(voiceUdpServerAddress, remotePort);
        }

        public async Task SendIpDiscoveryPacketAsync()
        {
            var ipDiscoveryPacket = new VoicePacket();
            ipDiscoveryPacket.Header.SynchronizationSourceId = _ssrcId;
            //ipDiscoveryPacket.Body.Initialize();
            var ipDiscoveryPacketBytes = ipDiscoveryPacket.GetBytes();

            var sendInt = await _udpClient.SendAsync(ipDiscoveryPacketBytes, ipDiscoveryPacketBytes.Length, _voiceUdpEndpoint);
            _logger.LogTrace("Return valuse of _udpClient.SendAsync(): " + sendInt);

            await ReceiveAsync();
        }

        async Task ReceiveAsync()
        {
            var udpReceiveResult = await _udpClient.ReceiveAsync();

            _logger.LogTrace($"Received {udpReceiveResult.Buffer.Length} bytes on Voice UDP Socket");
            _logger.LogTrace($"Received {udpReceiveResult.Buffer.ToString()} on Voice UDP Socket");
        }
    }
}