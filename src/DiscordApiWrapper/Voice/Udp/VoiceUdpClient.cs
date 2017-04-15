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

namespace DiscordApiWrapper.Voice
{
    class VoiceUdpClient
    {
        static readonly MyLogger _logger = new MyLogger(nameof(VoiceUdpClient), ConsoleColor.DarkGreen);

        UdpClient _udpClient;
        IPEndPoint _voiceUdpEndpoint;
        uint _synchronizationSourceId;
        public byte[] SecretKey;
        const int MaxOpusSize = 4000;
        const int millisecondsInASecond = 1000;
        readonly byte[] SILENCE_FRAMES = { 0xF8, 0xFF, 0xFE };
        readonly byte[] KEEP_ALIVE_DATA = { 0xC9, 0, 0, 0, 0, 0, 0, 0, 0 };
        const int headerSizeInBytes = 12;
        const int crytpoTagSizeInBytes = 16;

        public VoiceUdpClient(Uri remoteUri, int remotePort, uint synchronizationSourceId)
        {
            _udpClient = new UdpClient();
            _voiceUdpEndpoint = UdpUtility.GetEndpointFromInfo(remoteUri, remotePort);
            _synchronizationSourceId = synchronizationSourceId;
        }

        public async Task<Tuple<string, int>> SendIpDiscoveryPacketAsync()
        {
            var ipDiscoveryPacket = new VoicePacket(0, 0, _synchronizationSourceId, new byte[58]);

            await SendAsync(ipDiscoveryPacket.GetUnencryptedBytes());

            var IpDiscoveryResultBytes = await ReceiveAsync();
            _logger.LogTrace($"IP Discovery Response: {IpDiscoveryResultBytes.Length} bytes: {BitConverter.ToString(IpDiscoveryResultBytes)}");

            var IpDiscoveryResult = UdpUtility.GetIpAddressAndPortFromIpDiscoveryResponse(IpDiscoveryResultBytes);

            _logger.LogTrace($"Results of IP Discovery: Public IP Address: {IpDiscoveryResult.Item1}, Port: {IpDiscoveryResult.Item2}", ConsoleColor.Green);

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
                Buffer.BlockCopy(voicePacketHeader.GetBytes(), 0, encryptedBytes, 0, headerSizeInBytes);

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
            var opusEncoder = new ConcentusDemo.ConcentusCodec();
            var random = new Random();
            int samplingRate = 48000;
            int channels = 2;
            var frameLengthInMs = 20;
            uint samplesPerFrame = (uint)((samplingRate / millisecondsInASecond) * frameLengthInMs);
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

                var encryptedBytes = new byte[headerSizeInBytes + crytpoTagSizeInBytes + compressedBytes.Length];

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
            var opusEncoder = new ConcentusDemo.ConcentusCodec();
            var random = new Random();
            int samplingRate = 48000;
            int channels = 2;
            var frameLengthInMs = 20;
            uint samplesPerFrame = (uint)((samplingRate / millisecondsInASecond) * frameLengthInMs);
            opusEncoder.SetFrameSize(frameLengthInMs);
            ushort sequence = 0;
            uint timestamp = 0;
            int framesToSend = 100;

            // stopwatch
            double ticksPerMillisecond = Stopwatch.Frequency / 1000.0;
            double ticksPerFrame = ticksPerMillisecond * frameLengthInMs;

            double nextFrameInTicks = 0;

            //var randomNoisePCM = GenerateSquareWavePcm(channels, samplingRate, frameLengthInMs);
            //var randomNoisePCM = GenerateSinWavePcm(channels, samplingRate, frameLengthInMs);

            var wavReader = new WavFileReader();
            var fullSongPcm = wavReader.ReadFile(new FileInfo("ms.wav"));

            // read 20ms from song
            var shortsPerMs = (samplingRate * channels) / millisecondsInASecond;
            var shortsToRead = 20 * shortsPerMs;

            Stopwatch sw = Stopwatch.StartNew();
            
            var index = 0;
            var pcmFrame = new short[shortsToRead];

            Debug.Assert(samplesPerFrame == 960);

            while (true)
            {
                Buffer.BlockCopy(fullSongPcm, index, pcmFrame, 0, shortsToRead);

                index += shortsToRead;
                if (index > fullSongPcm.Length - shortsToRead)
                {
                    _logger.LogInfo($"Ran out of shorts to read, breaking!");
                    break;
                }

                var compressedBytes = opusEncoder.Compress(pcmFrame);

                var voicePacket = new VoicePacket(sequence, timestamp, _synchronizationSourceId, compressedBytes);

                // Find out how much time to wait
                double ticksUntilNextFrame = nextFrameInTicks - sw.ElapsedTicks;
                int msUntilNextFrame = (int)Math.Floor(ticksUntilNextFrame / ticksPerMillisecond);

                _logger.LogDebug($"msUntilNextFrame {msUntilNextFrame}");

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

        async Task SendFiveFramesOfSilence(ushort sequence, uint timestamp, uint samplesPerFrame)
        {
            var voicePacket2 = new VoicePacket(sequence, timestamp, _synchronizationSourceId, SILENCE_FRAMES);
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
            var pcm = new short[((samplingRate * channels) / millisecondsInASecond) * lengthInMs];

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
            var pcm = new short[((samplingRate * channels) / millisecondsInASecond) * lengthInMs];

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