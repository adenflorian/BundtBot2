using System;
using System.Diagnostics;
using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class ShittyDistortionAudioEffectStream : StreamWrapper
    {
        public Stream BasePcmAudioStream;

        public override bool CanRead => BasePcmAudioStream.CanRead;
        public override bool CanSeek => BasePcmAudioStream.CanSeek;
        public override bool CanWrite => BasePcmAudioStream.CanWrite;
        public override long Length => BasePcmAudioStream.Length;
        public override long Position { get => BasePcmAudioStream.Position; set => BasePcmAudioStream.Position = value; }

        static readonly MyLogger _logger = new MyLogger(nameof(SloMoAudioEffectStream));

        public ShittyDistortionAudioEffectStream(Stream pcmAudioStream)
        {
            BasePcmAudioStream = pcmAudioStream;
        }

        public override void Flush()
        {
            BasePcmAudioStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = BasePcmAudioStream.Read(buffer, offset, count);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(buffer[i] * 0.5f);
            }

            return result;
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