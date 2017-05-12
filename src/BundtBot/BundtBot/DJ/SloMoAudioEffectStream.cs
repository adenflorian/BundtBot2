using System;
using System.Diagnostics;
using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class SloMoAudioEffectStream : StreamWrapper
    {
        public Stream BasePcmAudioStream;

        public override bool CanRead => BasePcmAudioStream.CanRead;
        public override bool CanSeek => BasePcmAudioStream.CanSeek;
        public override bool CanWrite => BasePcmAudioStream.CanWrite;
        public override long Length => BasePcmAudioStream.Length;
        public override long Position { get => BasePcmAudioStream.Position; set => BasePcmAudioStream.Position = value; }

        static readonly MyLogger _logger = new MyLogger(nameof(SloMoAudioEffectStream));

        public SloMoAudioEffectStream(Stream pcmAudioStream)
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

            var extraBytesCount = 4;
            if ((count / 2) % 4 != 0) extraBytesCount += 2;

            var halfCountBuffer = new byte[(count / 2) + extraBytesCount];
            var readCountFromBaseStream = BasePcmAudioStream.Read(halfCountBuffer, offset, halfCountBuffer.Length);
            if (readCountFromBaseStream == 0) return 0;
            if (BasePcmAudioStream.Position != BasePcmAudioStream.Length)
            {
                BasePcmAudioStream.Position -= extraBytesCount;
            }

            var flag = true;
            var j = 0;
            for (int i = 0; i < buffer.Length; i += 4)
            {
                if (flag)
                {
                    buffer[i + 0] = halfCountBuffer[j + 0];
                    buffer[i + 1] = halfCountBuffer[j + 1];
                    buffer[i + 2] = halfCountBuffer[j + 2];
                    buffer[i + 3] = halfCountBuffer[j + 3];
                    flag = false;
                }
                else
                {
                    var leftShortCurrent = BitConverter.ToInt16(halfCountBuffer, j + 0);
                    var leftShortNext = BitConverter.ToInt16(halfCountBuffer, j + 4);
                    var leftAverage = (leftShortCurrent + leftShortNext) / 2;

                    var rightShortCurrent = BitConverter.ToInt16(halfCountBuffer, j + 2);
                    var rightShortNext = BitConverter.ToInt16(halfCountBuffer, j + 6);
                    var rightAverage = (rightShortCurrent + rightShortNext) / 2;

                    buffer[i + 0] = (byte)(leftAverage >> 0);
                    buffer[i + 1] = (byte)(leftAverage >> 8);
                    buffer[i + 2] = (byte)(rightAverage >> 0);
                    buffer[i + 3] = (byte)(rightAverage >> 8);
                    flag = true;
                    j += 4;
                }
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

        public override void SwapOutBaseStream(Stream newBaseStream)
        {
            BasePcmAudioStream?.Dispose();
            BasePcmAudioStream = newBaseStream;
        }
    }
}