namespace DiscordApiWrapper.Voice
{
    enum VoiceOpCode
    {
        /// <summary>used to begin a voice websocket connection</summary>
		Identify = 0,

        /// <summary>used to begin a voice websocket connection</summary>
        Select = 1,

        /// <summary>used to complete the websocket handshake</summary>
        Ready = 2,

        /// <summary>used to keep the websocket connection alive</summary>
        Heartbeat = 3,

        /// <summary>Description used to describe the session</summary>
        Session = 4,

        /// <summary>used to indicate which users are speaking</summary>
        Speaking = 5,

        Hello = 8
    }
}