using System;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
    public class VoiceServerHello
    {
        /// <summary>
		/// The interval (in milliseconds) the client should heartbeat with.
		/// </summary>
        [JsonProperty("heartbeat_interval")]
        int _heartbeatInterval;
        public TimeSpan HeartbeatInterval
        {
            get { return TimeSpan.FromMilliseconds(_heartbeatInterval); }
            set { _heartbeatInterval = value.Milliseconds; }
        }
    }
}