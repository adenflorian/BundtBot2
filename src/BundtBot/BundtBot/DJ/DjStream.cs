using System;
using System.IO;

namespace BundtBot
{
    // TODO Make disposable
    public class DjStream : Stream
    {
        public override bool CanRead => _wetPcmAudioStream.CanRead;
        public override bool CanSeek => _wetPcmAudioStream.CanSeek;
        public override bool CanWrite => _wetPcmAudioStream.CanWrite;
        public override long Length => _wetPcmAudioStream.Length;
        public override long Position { get => _wetPcmAudioStream.Position; set => _wetPcmAudioStream.Position = value; }

        Stream _originalBasePcmAudioStream;
        StreamWrapper _wetPcmAudioStream;
        float _volumeMod = 0.5f;

        public DjStream(Stream pcmAudioStream)
        {
            SwapOutBaseStream(pcmAudioStream);
        }

        public void SwapOutBaseStream(Stream newBaseStream)
        {
            _originalBasePcmAudioStream?.Dispose();
            _originalBasePcmAudioStream = newBaseStream;
            
            _wetPcmAudioStream?.Dispose();
            _wetPcmAudioStream = new NoFxStream(newBaseStream);
        }

        public void AddFastforwardEffect()
        {
            _wetPcmAudioStream = new FastForwardAudioEffectStream(_wetPcmAudioStream);
        }

        public void AddSloMoEffect()
        {
            _wetPcmAudioStream = new SloMoAudioEffectStream(_wetPcmAudioStream);
        }

        public void AddShittyEffect()
        {
            _wetPcmAudioStream = new ShittyDistortionAudioEffectStream(_wetPcmAudioStream);
        }

        public void RemoveEffects()
        {
            _wetPcmAudioStream = new NoFxStream(_originalBasePcmAudioStream);
        }

        public override void Flush()
        {
            _wetPcmAudioStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _wetPcmAudioStream.Read(buffer, offset, count);

            for (int i = 0; i < buffer.Length; i += 2)
            {
                var sample = BitConverter.ToInt16(buffer, i);
                sample = (short)(sample * _volumeMod);
                buffer[i] = (byte)(sample >> 0);
                buffer[i + 1] = (byte)(sample >> 8);
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _wetPcmAudioStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _wetPcmAudioStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _wetPcmAudioStream.Write(buffer, offset, count);
        }

        public void SetVolume(int newVolume)
        {
            if (newVolume > 10 || newVolume < 1)
            {
                throw new DJException("Volume must be between 1 and 10");
            }

            _volumeMod = newVolume / 10f;
        }
    }
}