using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Opus;
using DiscordApiWrapper.Voice.Udp;

namespace DiscordApiWrapper.Voice
{
    class VoiceUdpClient
    {
        public byte[] SecretKey;

        const int _maxOpusSize = 4000;
        const int _msPerSecond = 1000;
        const int _headerSizeInBytes = 12;
        const int _crytpoTagSizeInBytes = 16;

        static readonly MyLogger _logger = new MyLogger(nameof(VoiceUdpClient), ConsoleColor.DarkGreen);
        static readonly byte[] _silenceFrames = { 0xF8, 0xFF, 0xFE };
        static readonly byte[] _keepAliveData = { 0xC9, 0, 0, 0, 0, 0, 0, 0, 0 };
        static readonly double _ticksPerMillisecond = Stopwatch.Frequency / _msPerSecond;

        readonly UdpClient _udpClient;
        readonly IPEndPoint _voiceUdpEndpoint;
        readonly uint _syncSourceId;

        public VoiceUdpClient(Uri remoteUri, int remotePort, uint synchronizationSourceId)
        {
            _udpClient = new UdpClient();
            _voiceUdpEndpoint = UdpUtility.GetEndpointFromInfo(remoteUri, remotePort);
            _syncSourceId = synchronizationSourceId;
        }

        public async Task<IpDiscoveryResult> SendIpDiscoveryPacketAsync()
        {
            var ipDiscoveryPacket = new VoicePacket(0, 0, _syncSourceId, new byte[58]);

            await SendAsync(ipDiscoveryPacket.GetUnencryptedBytes());

            var IpDiscoveryResultBytes = await ReceiveAsync();
            _logger.LogTrace($"IP Discovery Response: {IpDiscoveryResultBytes.Length} bytes: {BitConverter.ToString(IpDiscoveryResultBytes)}");

            var IpDiscoveryResult = UdpUtility.GetIpAddressAndPortFromIpDiscoveryResponse(IpDiscoveryResultBytes);
            _logger.LogDebug($"Results of IP Discovery: Public IP Address: {IpDiscoveryResult.IpAddress}, Port: {IpDiscoveryResult.Port}", ConsoleColor.Green);

            return IpDiscoveryResult;
        }

        internal async Task SendAudioAsync(byte[] sodaBytes)
        {
            var samplingRate = 48000;
            var channels = 2;
            var frameLengthInMs = 20;
            uint samplesPerFramePerChannel = (uint)((samplingRate / _msPerSecond) * frameLengthInMs);

            var bitDepth = 16;
            var bytesPerSample = bitDepth / 8;

            ushort sequence = 0;
            uint timestamp = 0;

            var opusEncoder = OpusEncoder.Create(samplingRate, channels, Application.Audio);

            double ticksPerFrame = _ticksPerMillisecond * frameLengthInMs;
            double nextFrameInTicks = 0;

            var samplesPerMs = (samplingRate * channels) / _msPerSecond;
            var bytesToRead = 20 * samplesPerMs * bytesPerSample;

            Stopwatch sw = Stopwatch.StartNew();
            
            var index = 0;
            var pcmFrame = new byte[bytesToRead];

            while (true)
            {
                Buffer.BlockCopy(sodaBytes, index, pcmFrame, 0, bytesToRead);

                index += bytesToRead;
                if (index > sodaBytes.Length - bytesToRead)
                {
                    _logger.LogInfo($"Ran out of bytes to read, breaking out of loop");
                    break;
                }

                int encodedLength;

                var compressedBytes = opusEncoder.Encode(pcmFrame, pcmFrame.Length, out encodedLength);

                var compressedBytesShort = new byte[encodedLength];
                Buffer.BlockCopy(compressedBytes, 0, compressedBytesShort, 0, encodedLength);

                var voicePacket = new VoicePacket(sequence, timestamp, _syncSourceId, compressedBytesShort);

                // Find out how much time to wait
                double ticksUntilNextFrame = nextFrameInTicks - sw.ElapsedTicks;
                int msUntilNextFrame = (int)Math.Floor(ticksUntilNextFrame / _ticksPerMillisecond);

                if (msUntilNextFrame > 0)
                {
                    await Task.Delay(msUntilNextFrame);
                }

                await SendAsync(voicePacket.GetEncryptedBytes(SecretKey));

                timestamp += samplesPerFramePerChannel;
                sequence++;
                nextFrameInTicks += ticksPerFrame;
            }

            await SendFiveFramesOfSilence(sequence, timestamp, samplesPerFramePerChannel);
        }

        async Task SendFiveFramesOfSilence(ushort sequence, uint timestamp, uint samplesPerFrame)
        {
            var voicePacket = new VoicePacket(sequence, timestamp, _syncSourceId, _silenceFrames);
            for (int i = 0; i < 5; i++)
            {
                await SendAsync(voicePacket.GetEncryptedBytes(SecretKey));
                timestamp += samplesPerFrame;
                sequence++;
            }
        }

        async Task<int> SendAsync(byte[] bytesToSend)
        {
            var bytesSent = await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, _voiceUdpEndpoint);
            return bytesSent;
        }

        async Task<byte[]> ReceiveAsync()
        {
            var udpReceiveResult = await _udpClient.ReceiveAsync();
            return udpReceiveResult.Buffer;
        }
    }
}