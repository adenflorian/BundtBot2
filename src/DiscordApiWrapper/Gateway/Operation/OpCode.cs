namespace BundtBot.Discord.Gateway.Operation {
	public enum OpCode {
		/// <summary>
		/// Dispatches an event. "Dispatch"
		/// </summary>
		Dispatch = 0,

		/// <summary>
		/// Used for ping checking. "Heartbeat"
		/// </summary>
		Heartbeat = 1,

		/// <summary>
		/// Used for client handshake. "Identify"
		/// </summary>
		Identify = 2,

		/// <summary>
		/// Used to update the client status. "Status Update"
		/// </summary>
		StatusUpdate = 3,

		/// <summary>
		/// Used to join/move/leave voice channels. "Voice State Update"
		/// </summary>
		VoiceStateUpdate = 4,

		/// <summary>
		/// Used for voice ping checking. "Voice Server Ping"
		/// </summary>
		VoiceServerPing = 5,

		/// <summary>
		/// Used to resume a closed connection. "Resume"
		/// </summary>
		Resume = 6,

		/// <summary>
		/// Used to tell clients to reconnect to the gateway. "Reconnect"
		/// </summary>
		Reconnect = 7,

		/// <summary>
		/// Used to request guild members. "Request Guild Members"
		/// </summary>
		RequestGuildMembers = 8,

		/// <summary>
		/// Used to notify client they have an invalid session id. "Invalid Session"
		/// </summary>
		InvalidSession = 9,

		/// <summary>
		/// Sent immediately after connecting, contains heartbeat and server debug information. "Hello"
		/// </summary>
		Hello = 10,

		/// <summary>
		/// Sent immediately following a client heartbeat that was received. "Heartback ACK"
		/// </summary>
		HeartbackAck = 11
	}
}
