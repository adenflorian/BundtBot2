using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class DjStream : Stream
    {
        public readonly Stream OriginalBasePcmAudioStream;

        public Stream AffectedPcmAudioStream { get; private set; }

        public override bool CanRead => AffectedPcmAudioStream.CanRead;
        public override bool CanSeek => AffectedPcmAudioStream.CanSeek;
        public override bool CanWrite => AffectedPcmAudioStream.CanWrite;
        public override long Length => AffectedPcmAudioStream.Length;
        public override long Position { get => AffectedPcmAudioStream.Position; set => AffectedPcmAudioStream.Position = value; }

        public DjStream(Stream pcmAudioStream)
        {
            OriginalBasePcmAudioStream = pcmAudioStream;
            AffectedPcmAudioStream = pcmAudioStream;
        }

        public void EnableFastforward()
        {
            AffectedPcmAudioStream = new FastForwardAudioEffectStream(AffectedPcmAudioStream);
        }

        public void DisableFastforward()
        {
            AffectedPcmAudioStream = OriginalBasePcmAudioStream;
        }

        public override void Flush()
        {
            AffectedPcmAudioStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return AffectedPcmAudioStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return AffectedPcmAudioStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            AffectedPcmAudioStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AffectedPcmAudioStream.Write(buffer, offset, count);
        }
    }
}