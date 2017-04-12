using System;
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
        uint _synchronizationSourceId;

        public VoiceUdpClient(Uri remoteUri, int remotePort, uint synchronizationSourceId)
        {
            _udpClient = new UdpClient();
            _voiceUdpEndpoint = UdpUtility.GetEndpointFromInfo(remoteUri, remotePort);
            _synchronizationSourceId = synchronizationSourceId;
        }

        public async Task<Tuple<string, int>> SendIpDiscoveryPacketAsync()
        {
            var ipDiscoveryPacket = new VoicePacket(0, 0, _synchronizationSourceId);

            await SendAsync(ipDiscoveryPacket.GetBytes());

            var IpDiscoveryResultBytes = await ReceiveAsync();
            _logger.LogTrace($"IP Discovery Response: {IpDiscoveryResultBytes.Length} bytes: {BitConverter.ToString(IpDiscoveryResultBytes)}");

            var IpDiscoveryResult = UdpUtility.GetIpAddressAndPortFromIpDiscoveryResponse(IpDiscoveryResultBytes);

            _logger.LogTrace($"Results of IP Discovery: Public IP Address: {IpDiscoveryResult.Item1}, Port: {IpDiscoveryResult.Item2}", ConsoleColor.Green);

            return IpDiscoveryResult;
        }

        async Task<int> SendAsync(byte[] bytesToSend)
        {
            _logger.LogDebug($"Sending {bytesToSend.Length} bytes to {_voiceUdpEndpoint}");
            var bytesSent = await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, _voiceUdpEndpoint);
            _logger.LogDebug($"Sent {bytesSent} bytes");
            return bytesSent;
        }

        async Task<byte[]> ReceiveAsync()
        {
            var udpReceiveResult = await _udpClient.ReceiveAsync();

            _logger.LogDebug($"Received {udpReceiveResult.Buffer.Length} bytes");

            return udpReceiveResult.Buffer;
        }
    }
}