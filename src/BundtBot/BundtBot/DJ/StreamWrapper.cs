using System.IO;

namespace BundtBot
{
    public abstract class StreamWrapper : Stream
    {
         public abstract void SwapOutBaseStream(Stream newBaseStream);
    }
}