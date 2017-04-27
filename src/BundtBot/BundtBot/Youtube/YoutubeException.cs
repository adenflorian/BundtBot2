using System;
using System.Runtime.Serialization;

namespace BundtBot
{
    [Serializable]
    class YoutubeException : Exception
    {
        /// <summary>
        /// This message will be deisplayed to the user
        /// </summary>
        public YoutubeException(string message) : base(message)
        {
        }

        /// <summary>
        /// This message will be deisplayed to the user
        /// </summary>
        public YoutubeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}