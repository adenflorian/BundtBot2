using System;
using DiscordApiWrapper.Sodium;

namespace DiscordApiWrapper.Voice.Udp
{
    class VoicePacket
    {
        public VoiceUdpPacketHeader Header;
        public byte[] Payload;
        const int crytpoTagSizeInBytes = 16;

        public VoicePacket(ushort sequence, uint timestamp, uint synchronizationSourceId, byte[] payload)
        {
            Header = new VoiceUdpPacketHeader(sequence, timestamp, synchronizationSourceId);
            Payload = payload;
        }

        public byte[] GetEncryptedBytes(byte[] SecretKey)
        {
            var headerBytes = Header.GetBytes();
            var voicePacketBytes = new byte[headerBytes.Length + Payload.Length + crytpoTagSizeInBytes];

            headerBytes.CopyTo(voicePacketBytes, 0);

            var nonce = new byte[headerBytes.Length * 2];
            Buffer.BlockCopy(headerBytes, 0, nonce, 0, headerBytes.Length);

            var encryptResult = SecretBox.Encrypt(Payload, Payload.Length, voicePacketBytes, headerBytes.Length, nonce, SecretKey);

            return voicePacketBytes;
        }

        public byte[] GetUnencryptedBytes()
        {
            var headerBytes = Header.GetBytes();
            var voicePacketBytes = new byte[headerBytes.Length + Payload.Length];

            headerBytes.CopyTo(voicePacketBytes, 0);
            Payload.CopyTo(voicePacketBytes, headerBytes.Length);

            return voicePacketBytes;
        }
    }
}