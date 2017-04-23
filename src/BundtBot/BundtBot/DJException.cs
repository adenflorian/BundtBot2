using System;
using System.Runtime.Serialization;

namespace BundtBot
{
    [Serializable]
    public class DJException : Exception
    {
        public DJException() { }

        public DJException( string message ) : base( message ) { }

        public DJException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}