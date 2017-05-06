using System;
using System.Diagnostics;
using System.IO;
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
        const int _frameLengthInMs = 20;
        const int _bitDepth = 16;
        const int _bytesPerSample = _bitDepth / 8;
        const uint _samplesPerFramePerChannel = (uint)((_samplingRate / _msPerSecond) * _frameLengthInMs);
        const int _samplesPerMs = (_samplingRate * _channels) / _msPerSecond;
        const int _bytesPer20Ms = 20 * _samplesPerMs * _bytesPerSample;

        static readonly MyLogger _logger = new MyLogger(nameof(VoiceUdpClient), ConsoleColor.DarkGreen);
        static readonly byte[] _silenceFrames = { 0xF8, 0xFF, 0xFE };
        static readonly byte[] _keepAliveData = { 0xC9, 0, 0, 0, 0, 0, 0, 0, 0 };
        static readonly double _ticksPerMillisecond = Stopwatch.Frequency / _msPerSecond;
        static readonly double _ticksPerFrame = _ticksPerMillisecond * _frameLengthInMs;

        readonly UdpClient _udpClient;
        readonly IPEndPoint _voiceUdpEndpoint;
        readonly OpusEncoder _opusEncoder = OpusEncoder.Create(_samplingRate, _channels, Application.Audio);
        readonly uint _syncSourceId;

        bool _isDisposing;
        bool _isDisposed;
        bool _isPaused;
        Stopwatch stopwatch;

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

        public async Task PauseAsync()
        {
            _isPaused = true;
            stopwatch.Stop();
            await SendFiveFramesOfSilence(0, 0, 0);
        }

        public void Resume()
        {
            _isPaused = false;
            stopwatch.Start();
        }

        public async Task SendAudioAsync(Stream pcmAudioStream)
        {
            if (SecretKey == null) throw new InvalidOperationException("Secret Key is still null");

            ushort sequence = 0;
            uint timestamp = 0;
            double nextFrameInTicks = 0;
            var pcmFrame = new byte[_bytesPer20Ms];

            stopwatch = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        if (_isDisposing) return;
                        if (pcmAudioStream.Position == pcmAudioStream.Length) break;
                        if (_isPaused) { Thread.Sleep(100); continue; }

                        if (pcmAudioStream.CanRead == false) break;
                        if (pcmAudioStream.Position == pcmAudioStream.Length) break;
                        pcmAudioStream.Read(pcmFrame, 0, pcmFrame.Length);

                        int encodedLength;
                        var compressedBytes = _opusEncoder.Encode(pcmFrame, pcmFrame.Length, out encodedLength);

                        var compressedBytesShort = new byte[encodedLength];
                        Buffer.BlockCopy(compressedBytes, 0, compressedBytesShort, 0, encodedLength);

                        var voicePacket = new VoicePacket(sequence, timestamp, _syncSourceId, compressedBytesShort);
                        var encryptedVoicePacketBytes = voicePacket.GetEncryptedBytes(SecretKey);

                        int msUntilNextFrame = FindOutHowLongToWait(nextFrameInTicks);
                        if (msUntilNextFrame > 0) Thread.Sleep(msUntilNextFrame);

                        var task = SendAsync(encryptedVoicePacketBytes);

                        timestamp += _samplesPerFramePerChannel;
                        sequence++;
                        nextFrameInTicks += _ticksPerFrame;

                        if (sequence % 100 == 0)
                        {
                            _logger.LogTrace($"Sequence: {sequence}, Stream Position: {pcmAudioStream.Position}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception caught in send audio loop");
                    _logger.LogError(ex);
                }
            });

            await SendFiveFramesOfSilence(sequence, timestamp, _samplesPerFramePerChannel);
        }

        bool IsAtEndOfAudio(int index, int pcmAudioBytesLength)
        {
            // TODO This is inaccurate
            if (index > pcmAudioBytesLength - _bytesPer20Ms)
            {
                _logger.LogInfo($"Ran out of bytes to read, breaking out of loop");
                return true;
            }
            return false;
        }

        int FindOutHowLongToWait(double nextFrameInTicks)
        {
            double ticksUntilNextFrame = nextFrameInTicks - stopwatch.ElapsedTicks;
            return (int)Math.Floor(ticksUntilNextFrame / _ticksPerMillisecond);
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
            if (_isDisposing) return 0;
            var bytesSent = await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, _voiceUdpEndpoint);
            return bytesSent;
        }

        async Task<byte[]> ReceiveAsync()
        {
            if (_isDisposing) return null;
            var udpReceiveResult = await _udpClient.ReceiveAsync();
            return udpReceiveResult.Buffer;
        }

        ~VoiceUdpClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            _isDisposing = true;
            if (_isDisposed == false)
            {
                _logger.LogDebug("Disposing");
                _udpClient.Dispose();
                _opusEncoder.Dispose();
                _isDisposed = true;
            }
        }
    }
}