using System;
using System.Diagnostics;
using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class FastForwardAudioEffectStream : Stream
    {
        public readonly Stream BasePcmAudioStream;

        public override bool CanRead => BasePcmAudioStream.CanRead;
        public override bool CanSeek => BasePcmAudioStream.CanSeek;
        public override bool CanWrite => BasePcmAudioStream.CanWrite;
        public override long Length => BasePcmAudioStream.Length;
        public override long Position { get => BasePcmAudioStream.Position; set => BasePcmAudioStream.Position = value; }

        public FastForwardAudioEffectStream(Stream pcmAudioStream)
        {
            BasePcmAudioStream = pcmAudioStream;
        }

        public override void Flush()
        {
            BasePcmAudioStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Debug.Assert(count % 4 == 0);
            
            // Read twice as many bytes from base stream
            var twiceCountBuffer = new byte[count * 2];
            var bytesReadFromBaseStream = BasePcmAudioStream.Read(twiceCountBuffer, offset, count * 2);
            if (bytesReadFromBaseStream == 0) return 0;

            // Remove every other 4 bytes (2 byte sper sample per channel (2 channels))
            var j = 0;
            for (int i = 0; i < count; i += 4)
            {
                buffer[i] = twiceCountBuffer[j];
                buffer[i + 1] = twiceCountBuffer[j + 1];
                buffer[i + 2] = twiceCountBuffer[j + 2];
                buffer[i + 3] = twiceCountBuffer[j + 3];

                j += 8;
            }

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BasePcmAudioStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BasePcmAudioStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BasePcmAudioStream.Write(buffer, offset, count);
        }
    }
}