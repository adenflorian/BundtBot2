namespace DiscordApiWrapper.Gateway {
	enum GatewayOpCode {
		/// <summary>Dispatches an event</summary>
		Dispatch = 0,

		/// <summary>Used for ping checking</summary>
		Heartbeat = 1,

		/// <summary>Used for client handshake</summary>
		Identify = 2,

		/// <summary>Used to update the client status</summary>
		StatusUpdate = 3,

		/// <summary>Used to join/move/leave voice channels</summary>
		VoiceStateUpdate = 4,

		/// <summary>Used for voice ping checking</summary>
		VoiceServerPing = 5,

		/// <summary>Used to resume a closed connection</summary>
		Resume = 6,

		/// <summary>Used to tell clients to reconnect to the gateway</summary>
		Reconnect = 7,

		/// <summary>Used to request guild members</summary>
		RequestGuildMembers = 8,

		/// <summary>Used to notify client they have an invalid session id</summary>
		InvalidSession = 9,

		/// <summary>Sent immediately after connecting, 
		/// contains heartbeat and server debug information</summary>
		Hello = 10,

		/// <summary>Sent immediately following a client heartbeat that was received</summary>
		HeartbeatAck = 11
	}
}
