using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BundtBot;
using DiscordApiWrapper.Audio;
using DiscordApiWrapper.Sodium;
using DiscordApiWrapper.Voice.Udp;
using FragLabs.Audio.Codecs;
using FragLabs.Audio.Codecs.Opus;

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
        readonly uint _synchronizationSourceId;

        public VoiceUdpClient(Uri remoteUri, int remotePort, uint synchronizationSourceId)
        {
            _udpClient = new UdpClient();
            _voiceUdpEndpoint = UdpUtility.GetEndpointFromInfo(remoteUri, remotePort);
            _synchronizationSourceId = synchronizationSourceId;
        }

        public async Task<IpDiscoveryResult> SendIpDiscoveryPacketAsync()
        {
            var ipDiscoveryPacket = new VoicePacket(0, 0, _synchronizationSourceId, new byte[58]);

            await SendAsync(ipDiscoveryPacket.GetUnencryptedBytes());

            var IpDiscoveryResultBytes = await ReceiveAsync();
            _logger.LogTrace($"IP Discovery Response: {IpDiscoveryResultBytes.Length} bytes: {BitConverter.ToString(IpDiscoveryResultBytes)}");

            var IpDiscoveryResult = UdpUtility.GetIpAddressAndPortFromIpDiscoveryResponse(IpDiscoveryResultBytes);
            _logger.LogDebug($"Results of IP Discovery: Public IP Address: {IpDiscoveryResult.IpAddress}, Port: {IpDiscoveryResult.Port}", ConsoleColor.Green);

            return IpDiscoveryResult;
        }

        internal async Task SendAudioAsync(byte[] sodaBytes)
        {
            //await SendVoiceEchoAsync();
            //await SendVoiceNoiseAsync();
            await SendMusicAsync();
        }

        async Task SendVoiceEchoAsync()
        {
            //uint timestamp = 0;
            //const int frameSize = 120;
            /*for (int i = 0; i < frameSize * 50; i += frameSize)
            {
                var newSendArray = new byte[frameSize];
                for (int j = 0; j < frameSize; j++)
                {
                    newSendArray[j] = sodaBytes[j + (i)];
                }
                const uint crypto_secretbox_xsalsa20poly1305_MACBYTES = 16;
                var encryptedBytes = new byte[newSendArray.Length + crypto_secretbox_xsalsa20poly1305_MACBYTES];
                var vp = new VoicePacket(_sequence, timestamp, _synchronizationSourceId);
                var headerBytes = vp.Header.GetBytes();
                var nonce = new byte[24];
                headerBytes.CopyTo(nonce, 0);
                _logger.LogDebug($"nonce: {BitConverter.ToString(nonce)}");
                _logger.LogDebug($"SecretKey: {BitConverter.ToString(SecretKey)}");


                _logger.LogDebug($"pre encryption: {BitConverter.ToString(newSendArray)}");
                var encryptResult = SecretBox.Encrypt(newSendArray, newSendArray.Length, encryptedBytes, 0, nonce, SecretKey);
                _logger.LogDebug($"encryptResult: {encryptResult}, encrypted: {BitConverter.ToString(encryptedBytes)}");

                byte[] decryptedBytes = new byte[newSendArray.Length];

                var decryyptResult = SecretBox.Decrypt(encryptedBytes, 0, encryptedBytes.Length, decryptedBytes, nonce, SecretKey);
                _logger.LogDebug($"decryyptResult: {decryyptResult}, decrypted: {BitConverter.ToString(decryptedBytes)}");
                vp.Body = encryptedBytes;
                await SendAsync(vp.GetBytes());
                await Task.Delay(20);
                _sequence++;
                timestamp += 1000;
            }*/

            ushort seq = 0;
            uint timestamp = 0;

            while (seq < 10000)
            {
                var result = await ReceiveAsync();

                var nonce = new byte[24];

                Buffer.BlockCopy(result, 0, nonce, 0, 12);

                var decryptedBytes = new byte[result.Length - 12 - 16];

                var decryyptResult = SecretBox.Decrypt(result, 12, result.Length - 12, decryptedBytes, nonce, SecretKey);

                var encryptedBytes = new byte[12 + 16 + decryptedBytes.Length];

                var voicePacketHeader = new VoiceUdpPacketHeader(seq, timestamp, _synchronizationSourceId);
                Buffer.BlockCopy(voicePacketHeader.GetBytes(), 0, encryptedBytes, 0, _headerSizeInBytes);

                var nonce2 = new byte[24];

                Buffer.BlockCopy(encryptedBytes, 0, nonce2, 0, 12);

                var encryptResult = SecretBox.Encrypt(decryptedBytes, decryptedBytes.Length, encryptedBytes, 12, nonce2, SecretKey);

                await SendAsync(encryptedBytes);
                timestamp += 960;
                await Task.Delay(2);
                seq++;
            }
        }

        async Task SendVoiceNoiseAsync()
        {
            var opusEncoder = new ConcentusDemo.ConcentusCodec(2);
            var random = new Random();
            int samplingRate = 48000;
            int channels = 2;
            var frameLengthInMs = 20;
            uint samplesPerFrame = (uint)((samplingRate / _msPerSecond) * frameLengthInMs);
            opusEncoder.SetFrameSize(frameLengthInMs);
            ushort sequence = 0;
            uint timestamp = 0;
            int framesToSend = 100;

            // stopwatch
            double ticksPerMillisecond = Stopwatch.Frequency / 1000.0;
            double ticksPerFrame = ticksPerMillisecond * frameLengthInMs;

            double nextFrameInTicks = 0;

            var randomNoisePCM = GenerateSquareWavePcm(channels, samplingRate, frameLengthInMs);
            //var randomNoisePCM = GenerateSinWavePcm(channels, samplingRate, frameLengthInMs);

            Stopwatch sw = Stopwatch.StartNew();

            while (sequence < framesToSend)
            {
                var compressedBytes = opusEncoder.Compress(randomNoisePCM);

                var encryptedBytes = new byte[_headerSizeInBytes + _crytpoTagSizeInBytes + compressedBytes.Length];

                var voicePacket = new VoicePacket(sequence, timestamp, _synchronizationSourceId, compressedBytes);

                // Find out how much time to wait
                double ticksUntilNextFrame = nextFrameInTicks - sw.ElapsedTicks;
                int msUntilNextFrame = (int)Math.Floor(ticksUntilNextFrame / ticksPerMillisecond);

                if (msUntilNextFrame > 0)
                {
                    await Task.Delay(msUntilNextFrame);
                }

                await SendAsync(voicePacket.GetEncryptedBytes(SecretKey));

                timestamp += samplesPerFrame;
                sequence++;
                nextFrameInTicks += ticksPerFrame;
            }

            await SendFiveFramesOfSilence(sequence, timestamp, samplesPerFrame);

            var waitAmountMs = frameLengthInMs * framesToSend;
            _logger.LogInfo($"Waiting for {waitAmountMs} ms");
            await Task.Delay(waitAmountMs);
            _logger.LogInfo($"Done waiting for {waitAmountMs} ms");
        }

        async Task SendMusicAsync()
        {
            int samplingRate = 48000;
            int channels = 2;
            var frameLengthInMs = 20;
            uint samplesPerFramePerChannel = (uint)((samplingRate / _msPerSecond) * frameLengthInMs);
            Debug.Assert(samplesPerFramePerChannel == 960);

            int bytesPerSample = 2;

            ushort sequence = 0;
            uint timestamp = 0;
            
            const int framesToSend = 100;

            var opusEncoder = OpusEncoder.Create(samplingRate, channels, Application.Audio);

            // stopwatch
            double ticksPerFrame = _ticksPerMillisecond * frameLengthInMs;

            double nextFrameInTicks = 0;

            //var randomNoisePCM = GenerateSquareWavePcm(channels, samplingRate, frameLengthInMs);
            //var randomNoisePCM = GenerateSinWavePcm(channels, samplingRate, frameLengthInMs);

            var wavReader = new WavFileReader();
            var fullSongPcm = wavReader.ReadFileBytes(new FileInfo("ms.wav"));

            // make it mono
            // var monoPcm = new byte[fullSongPcm.Length / 2];

            // for (int i = 0; i < monoPcm.Length; i++)
            // {
            //     monoPcm[i] = fullSongPcm[i * 2];
            // }

            // read 20ms from song
            var samplesPerMs = (samplingRate * channels) / _msPerSecond;
            var shortsToRead = 20 * samplesPerMs * bytesPerSample;

            Stopwatch sw = Stopwatch.StartNew();
            
            var index = 0;
            var pcmFrame = new byte[shortsToRead];


            while (true)
            {
                Buffer.BlockCopy(fullSongPcm, index, pcmFrame, 0, shortsToRead);

                index += shortsToRead;
                if (index > fullSongPcm.Length - shortsToRead)
                {
                    _logger.LogInfo($"Ran out of shorts to read, breaking!");
                    break;
                }

                int encodedLength;


                _logger.LogInfo($"Encoding pcm frame of {pcmFrame.Length} bytes");
                var compressedBytes = opusEncoder.Encode(pcmFrame, pcmFrame.Length, out encodedLength);
                _logger.LogInfo($"encodedLength: {encodedLength}");


                var compressedBytesShort = new byte[encodedLength];
                Buffer.BlockCopy(compressedBytes, 0, compressedBytesShort, 0, encodedLength);

                var voicePacket = new VoicePacket(sequence, timestamp, _synchronizationSourceId, compressedBytesShort);

                // Find out how much time to wait
                double ticksUntilNextFrame = nextFrameInTicks - sw.ElapsedTicks;
                int msUntilNextFrame = (int)Math.Floor(ticksUntilNextFrame / _ticksPerMillisecond);

                _logger.LogDebug($"msUntilNextFrame {msUntilNextFrame}");

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

            var waitAmountMs = frameLengthInMs * framesToSend;
            _logger.LogInfo($"Waiting for {waitAmountMs} ms");
            await Task.Delay(waitAmountMs);
            _logger.LogInfo($"Done waiting for {waitAmountMs} ms");
        }

        async Task SendFiveFramesOfSilence(ushort sequence, uint timestamp, uint samplesPerFrame)
        {
            var voicePacket2 = new VoicePacket(sequence, timestamp, _synchronizationSourceId, _silenceFrames);
            await SendAsync(voicePacket2.GetEncryptedBytes(SecretKey));
            timestamp += samplesPerFrame;
            sequence++;
            await SendAsync(voicePacket2.GetEncryptedBytes(SecretKey));
            timestamp += samplesPerFrame;
            sequence++;
            await SendAsync(voicePacket2.GetEncryptedBytes(SecretKey));
            timestamp += samplesPerFrame;
            sequence++;
            await SendAsync(voicePacket2.GetEncryptedBytes(SecretKey));
            timestamp += samplesPerFrame;
            sequence++;
            await SendAsync(voicePacket2.GetEncryptedBytes(SecretKey));
            timestamp += samplesPerFrame;
            sequence++;
        }

        short[] GenerateSquareWavePcm(int channels, int samplingRate, int lengthInMs)
        {
            var pcm = new short[((samplingRate * channels) / _msPerSecond) * lengthInMs];

            for (int time = 0; time < pcm.Length / channels; time++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    if (time % 200 > 100)
                    {
                        pcm[(time * channels) + channel] = 4000;
                    }
                    else
                    {
                        pcm[(time * channels) + channel] = -4000;
                    }
                }
            }

            return pcm;
        }

        short[] GenerateSinWavePcm(int channels, int samplingRate, int lengthInMs)
        {
            var pcm = new short[((samplingRate * channels) / _msPerSecond) * lengthInMs];

            for (int time = 0; time < pcm.Length / channels; time++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    pcm[(time * channels) + channel] = (short)(Math.Sin(time / 50) * 4000);
                }
            }

            return pcm;
        }

        async Task<int> SendAsync(byte[] bytesToSend)
        {
            //_logger.LogDebug($"Sending {bytesToSend.Length} bytes to {_voiceUdpEndpoint}");
            //_logger.LogTrace($"Sending {bytesToSend.Length} bytes to {_voiceUdpEndpoint}: {BitConverter.ToString(bytesToSend)}");
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