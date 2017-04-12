using System;

namespace DiscordApiWrapper.Voice
{
    public class VoiceUdpPacketHeader
    {
        /* 
         * | Field     | Type                        | Size    |
         * |-----------|-----------------------------|---------|
         * | Type      | Single byte value of 0x80   | 1 byte  |
         * | Version   | Single byte value of 0x78   | 1 byte  |
         * | Sequence  | unsigned short (big endian) | 2 bytes |
         * | Timestamp | unsigned int (big endian)   | 4 bytes |
         * | SSRC      | unsigned int (big endian)   | 4 bytes |
         */

        readonly byte _type = 0x80;
        readonly byte _version = 0x78;
        ushort _sequence = 0;
        uint _timestamp = 0;
        public uint _synchronizationSourceId;

        public VoiceUdpPacketHeader(ushort sequence, uint timestamp, uint synchronizationSourceId)
        {
            _sequence = sequence;
            _timestamp = timestamp;
            _synchronizationSourceId = synchronizationSourceId;
        }

        public byte[] GetBytes()
        {
            var byteArray = new byte[1 + 1 + 2 + 4 + 4];
            byteArray[0] = _type;
            byteArray[1] = _version;

            var sequenceBytes = BitConverter.GetBytes(_sequence);
            byteArray[2] = sequenceBytes[0];
            byteArray[3] = sequenceBytes[1];

            var timestampBytes = BitConverter.GetBytes(_timestamp);
            byteArray[4] = timestampBytes[0];
            byteArray[5] = timestampBytes[1];
            byteArray[6] = timestampBytes[2];
            byteArray[7] = timestampBytes[3];

            var ssrcBytes = BitConverter.GetBytes(_synchronizationSourceId);
            byteArray[8] = ssrcBytes[0];
            byteArray[9] = ssrcBytes[1];
            byteArray[10] = ssrcBytes[2];
            byteArray[11] = ssrcBytes[3];

            return byteArray;
        }
    }
}