using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Opus;
using DiscordApiWrapper.Voice.Udp;

namespace DiscordApiWrapper.Voice
{
    class VoiceUdpClient : IDisposable
    {
        public byte[] SecretKey;

        const int _maxOpusSize = 4000;
        const int _msPerSecond = 1000;
        const int _headerSizeInBytes = 12;
        const int _crytpoTagSizeInBytes = 16;
        const int _samplingRate = 48000;
        const int _channels = 2;

        static readonly MyLogger _logger = new MyLogger(nameof(VoiceUdpClient), ConsoleColor.DarkGreen);
        static readonly byte[] _silenceFrames = { 0xF8, 0xFF, 0xFE };
        static readonly byte[] _keepAliveData = { 0xC9, 0, 0, 0, 0, 0, 0, 0, 0 };
        static readonly double _ticksPerMillisecond = Stopwatch.Frequency / _msPerSecond;

        readonly UdpClient _udpClient;
        readonly IPEndPoint _voiceUdpEndpoint;
        readonly OpusEncoder _opusEncoder = OpusEncoder.Create(_samplingRate, _channels, Application.Audio);
        readonly uint _syncSourceId;

        bool _isDisposed = false;
        bool _isPaused = false;
        Stopwatch sw;

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

        internal async Task PauseAsync()
        {
            _isPaused = true;
            sw.Stop();
            await SendFiveFramesOfSilence(0, 0, 0);
        }

        internal void Resume()
        {
            _isPaused = false;
            sw.Start();
        }

        internal async Task SendAudioAsync(byte[] pcmAudioBytes)
        {
            if (SecretKey == null) throw new InvalidOperationException("Secret Key is still null");

            var frameLengthInMs = 20;
            uint samplesPerFramePerChannel = (uint)((_samplingRate / _msPerSecond) * frameLengthInMs);

            var bitDepth = 16;
            var bytesPerSample = bitDepth / 8;

            ushort sequence = 0;
            uint timestamp = 0;

            double ticksPerFrame = _ticksPerMillisecond * frameLengthInMs;
            double nextFrameInTicks = 0;

            var samplesPerMs = (_samplingRate * _channels) / _msPerSecond;
            var bytesToRead = 20 * samplesPerMs * bytesPerSample;

            sw = Stopwatch.StartNew();
            
            var index = 0;
            var pcmFrame = new byte[bytesToRead];

            await Task.Run(() => {

                while (true)
                {
                    if (_isDisposed) return;
                    if (_isPaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    Buffer.BlockCopy(pcmAudioBytes, index, pcmFrame, 0, bytesToRead);

                    index += bytesToRead;
                    if (index > pcmAudioBytes.Length - bytesToRead)
                    {
                        _logger.LogInfo($"Ran out of bytes to read, breaking out of loop");
                        break;
                    }

                    int encodedLength;

                    var compressedBytes = _opusEncoder.Encode(pcmFrame, pcmFrame.Length, out encodedLength);

                    var compressedBytesShort = new byte[encodedLength];
                    Buffer.BlockCopy(compressedBytes, 0, compressedBytesShort, 0, encodedLength);

                    var voicePacket = new VoicePacket(sequence, timestamp, _syncSourceId, compressedBytesShort);

                    // Find out how much time to wait
                    double ticksUntilNextFrame = nextFrameInTicks - sw.ElapsedTicks;
                    int msUntilNextFrame = (int)Math.Floor(ticksUntilNextFrame / _ticksPerMillisecond);

                    _logger.LogTrace($"msUntilNextFrame: {msUntilNextFrame}");

                    if (msUntilNextFrame > 0)
                    {
                        _logger.LogTrace($"Before Thread.Sleep()");
                        Thread.Sleep(msUntilNextFrame);
                        //await Task.Delay(msUntilNextFrame);
                        _logger.LogTrace($"After Thread.Sleep()");
                    }
                    
                    _logger.LogTrace($"Before SendAsync");
                    // TODO Maybe check if previous send is still incomplete, and if it is, cancel it?
                    var task = SendAsync(voicePacket.GetEncryptedBytes(SecretKey));
                    _logger.LogTrace($"After  SendAsync");

                    timestamp += samplesPerFramePerChannel;
                    sequence++;
                    nextFrameInTicks += ticksPerFrame;
                }
            });

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

        ~VoiceUdpClient()
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
                    _udpClient.Dispose();
                    _opusEncoder.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}