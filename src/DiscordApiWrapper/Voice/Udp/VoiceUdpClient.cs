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

            _logger.LogTrace($"Sending {ipDiscoveryPacketBytes.Length} bytes to {_voiceUdpEndpoint}");
            var bytesSent = await _udpClient.SendAsync(ipDiscoveryPacketBytes, ipDiscoveryPacketBytes.Length, _voiceUdpEndpoint);
            _logger.LogTrace($"Send {bytesSent} bytes");

            var receivedBytes = await ReceiveAsync();

            // Get ip address and port
            var ipString = "";
            var i = 4;
            while (true)
            {
                if (receivedBytes[i] == 0x00) break;
                ipString += (char)receivedBytes[i];
                i++;
            }

            var port = 0;

            if (BitConverter.IsLittleEndian)
            {
                var portBytesLittleEndian = new byte[] { receivedBytes[receivedBytes.Length - 2], receivedBytes[receivedBytes.Length - 1] };
                port = BitConverter.ToUInt16(portBytesLittleEndian, 0);
            }
            else
            {
                var portBytesBigEndian = new byte[] { receivedBytes[receivedBytes.Length - 1], receivedBytes[receivedBytes.Length - 2] };
                port = BitConverter.ToUInt16(portBytesBigEndian, 0);
            }

            _logger.LogTrace($"Results of IP Discovery: Public IP Address: {ipString}, Port: {port}");
        }

        async Task<byte[]> ReceiveAsync()
        {
            var udpReceiveResult = await _udpClient.ReceiveAsync();

            _logger.LogTrace($"Received {udpReceiveResult.Buffer.Length} bytes on Voice UDP Socket: {BitConverter.ToString(udpReceiveResult.Buffer)}");

            return udpReceiveResult.Buffer;
        }
    }
}