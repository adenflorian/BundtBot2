using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class DjStream : Stream
    {
        Stream OriginalBasePcmAudioStream;
        StreamWrapper WetPcmAudioStream;

        public override bool CanRead => WetPcmAudioStream.CanRead;
        public override bool CanSeek => WetPcmAudioStream.CanSeek;
        public override bool CanWrite => WetPcmAudioStream.CanWrite;
        public override long Length => WetPcmAudioStream.Length;
        public override long Position { get => WetPcmAudioStream.Position; set => WetPcmAudioStream.Position = value; }

        public DjStream(Stream pcmAudioStream)
        {
            SwapOutBaseStream(pcmAudioStream);
        }

        public void SwapOutBaseStream(Stream newBaseStream)
        {
            OriginalBasePcmAudioStream?.Dispose();
            OriginalBasePcmAudioStream = newBaseStream;
            
            WetPcmAudioStream?.Dispose();
            WetPcmAudioStream = new NoFxStream(newBaseStream);
        }

        public void AddFastforwardEffect()
        {
            WetPcmAudioStream = new FastForwardAudioEffectStream(WetPcmAudioStream);
        }

        public void AddSloMoEffect()
        {
            WetPcmAudioStream = new SloMoAudioEffectStream(WetPcmAudioStream);
        }

        public void RemoveEffects()
        {
            WetPcmAudioStream = new NoFxStream(OriginalBasePcmAudioStream);
        }

        public override void Flush()
        {
            WetPcmAudioStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return WetPcmAudioStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return WetPcmAudioStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            WetPcmAudioStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WetPcmAudioStream.Write(buffer, offset, count);
        }
    }
}