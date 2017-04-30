using System;
using System.IO;

namespace BundtBot
{
    public class DummyStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => long.MaxValue;

        public override long Position { get => 0; set { } }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[i + offset] = 0;
            }
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}