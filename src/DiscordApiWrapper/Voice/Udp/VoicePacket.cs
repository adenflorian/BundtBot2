namespace DiscordApiWrapper.Voice.Udp
{
    class VoicePacket
    {
        public VoiceUdpPacketHeader Header;
        public byte[] Body = new byte[58];

        public VoicePacket(ushort sequence, uint timestamp, uint synchronizationSourceId)
        {
            Header = new VoiceUdpPacketHeader(sequence, timestamp, synchronizationSourceId);
        }

        public byte[] GetBytes()
        {
            var headerBytes = Header.GetBytes();
            var voicePacketBytes = new byte[headerBytes.Length + Body.Length];

            headerBytes.CopyTo(voicePacketBytes, 0);
            Body.CopyTo(voicePacketBytes, headerBytes.Length);
            
            return voicePacketBytes;
        }
    }
}