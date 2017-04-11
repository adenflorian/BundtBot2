using System;
using Newtonsoft.Json;

namespace DiscordApiWrapper.Voice
{
    public class VoiceServerReady
    {
        [JsonProperty("ssrc")]
        public int SynchronizationSourceId;

        [JsonProperty("port")]
        public int Port;

        [JsonProperty("modes")]
        public string[] Modes;

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