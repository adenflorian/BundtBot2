using System;
using System.Runtime.Serialization;

namespace BundtBot
{
    [Serializable]
    class YoutubeException : Exception
    {
        public YoutubeException()
        {
        }

        public YoutubeException(string message) : base(message)
        {
        }

        public YoutubeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}