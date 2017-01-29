using System;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Events;
using BundtBot.Discord.Models.Gateway;
using BundtBot.Extensions;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway
{
	public class DiscordGatewayClient
	{
		public delegate void OperationHandler(string eventName, string eventJsonData);
		public event OperationHandler DispatchReceived;
		public event OperationHandler HeartbackAckReceived;
		public event OperationHandler HelloReceived;
		public delegate void ReadyHandler(Ready readyInfo);
		public event ReadyHandler Ready;
		public delegate void MessageCreatedHandler(DiscordMessage discordMessage);
		public event MessageCreatedHandler MessageCreated;
		public delegate void GuildCreatedHandler(DiscordGuild discordGuild);
		/// <summary>
		/// This event can be sent in three different scenarios:
		///   1. When a user is initially connecting, to lazily load and backfill
		///      information for all unavailable guilds sent in the ready event.
		///   2. When a Guild becomes available again to the client.
		///   3. When the current user joins a new Guild.
		/// The inner payload is a guild object, with all the extra fields specified.
		/// </summary>
		public event GuildCreatedHandler GuildCreated;

		readonly ClientWebSocketWrapper _clientWebSocketWrapper = new ClientWebSocketWrapper();
		readonly MyLogger _logger = new MyLogger(nameof(DiscordGatewayClient));
		readonly string _authToken;

		int _lastSequenceReceived;

		public DiscordGatewayClient(string authToken)
		{
			_authToken = authToken;
			HelloReceived += OnHelloReceived;
			HeartbackAckReceived += HeartbackAckOperation.Instance.Execute;
			DispatchReceived += OnDispatchReceived;
			_clientWebSocketWrapper.MessageReceived += OnMessageReceived;
		}

		public async Task ConnectAsync(Uri gatewayUrl)
		{
			var modifiedGatewayUrl = gatewayUrl.AddParameter("v", "5").AddParameter("encoding", "'json'");
			await _clientWebSocketWrapper.ConnectAsync(modifiedGatewayUrl);
			_logger.LogInfo($"Connected to Gateway", ConsoleColor.Green);
		}
		
		async void OnHelloReceived(string eventName, string eventData)
		{
			_logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
			var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
			StartHeartBeatLoop(hello.HeartbeatInterval);
			await SendGatewayIdentify();
		}

		void StartHeartBeatLoop(TimeSpan heartbeatInterval)
		{
			Task.Run(async () => {
				while (true) {
					await SendHeartBeatAsync();
					await Task.Delay(heartbeatInterval);
				}
			});
			_logger.LogInfo("Heartbeat loop started", ConsoleColor.Green);
		}

		public async Task SendHeartBeatAsync()
		{
			_logger.LogInfo("Sending Heartbeat ♥ →");
			await SendOpCodeAsync(OpCode.Heartbeat, _lastSequenceReceived);
		}

		async Task SendGatewayIdentify()
		{
			_logger.LogInfo("Sending GatewayIdentify to Gateway", ConsoleColor.Green);
			await SendOpCodeAsync(OpCode.Identify, new GatewayIdentify {
				AuthenticationToken = _authToken,
				ConnectionProperties = new ConnectionProperties {
					OperatingSystem = "windows",
					Browser = "bundtbot",
					Device = "bundtbot",
					Referrer = "",
					ReferringDomain = "",
				},
				SupportsCompression = false,
				LargeThreshold = Threshold.Maximum
			});
		}

		public async Task SendStatusUpdate(StatusUpdate statusUpdate)
		{
			_logger.LogInfo("Sending StatusUpdate to Gateway" +
			                $"(idle since: {statusUpdate.IdleSince}, " +
			                $"game: {statusUpdate.Game.Name}",
							ConsoleColor.Green);
			await SendOpCodeAsync(OpCode.StatusUpdate, statusUpdate);
		}

		async Task SendOpCodeAsync(OpCode opCode, object eventData)
		{
			var gatewayPayload = new GatewayPayload(opCode, eventData);
			var jsonGatewayPayload = gatewayPayload.Serialize();

			_logger.LogDebug($"Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
			_logger.LogDebug("" + jsonGatewayPayload);

			await _clientWebSocketWrapper.SendMessageUsingQueueAsync(jsonGatewayPayload);

			_logger.LogDebug($"Sent {gatewayPayload.GatewayOpCode}");
		}

		void OnMessageReceived(string message)
		{
			var payload = JsonConvert.DeserializeObject<GatewayPayload>(message);

			StoreSequenceNumberForHeartbeat(payload);

			LogMessageReceived(message, payload);

			switch (payload.GatewayOpCode) {
				case OpCode.Dispatch: InvokeEvent(DispatchReceived, payload); break;
				case OpCode.HeartbackAck: InvokeEvent(HeartbackAckReceived, payload); break;
				case OpCode.Hello: InvokeEvent(HelloReceived, payload); break;
				case OpCode.Heartbeat:
				case OpCode.Identify:
				case OpCode.StatusUpdate:
				case OpCode.VoiceStateUpdate:
				case OpCode.VoiceServerPing:
				case OpCode.Resume:
				case OpCode.Reconnect:
				case OpCode.RequestGuildMembers:
				case OpCode.InvalidSession:
				default:
					_logger.LogWarning($"Received an OpCode with no handler: {payload.GatewayOpCode}");
					break;
			}
		}

		void LogMessageReceived(string message, GatewayPayload payload)
		{
			_logger.LogDebug($"Message received from gateway (opcode: {payload.GatewayOpCode})");
			_logger.LogDebug(message.Prettify());
		}

		void StoreSequenceNumberForHeartbeat(GatewayPayload receivedGatewayDispatch)
		{
			if (receivedGatewayDispatch.SequenceNumber.HasValue) {
				_lastSequenceReceived = receivedGatewayDispatch.SequenceNumber.Value;
			}
		}

		void InvokeEvent(OperationHandler handler, GatewayPayload payload)
		{
			handler?.Invoke(payload.EventName, payload.EventData?.ToString());
		}

		void OnDispatchReceived(string eventName, string eventJsonData)
		{
			_logger.LogDebug("Processing Gateway Event " + eventName);
				switch (eventName) {
					case "CHANNEL_CREATE":
						var channel = JsonConvert.DeserializeObject<Channel>(eventJsonData);
						_logger.LogInfo("Received Event: CHANNEL_CREATE " + channel.Id, ConsoleColor.Green);
						break;
					case "MESSAGE_CREATE":
						var discordMessage = JsonConvert.DeserializeObject<DiscordMessage>(eventJsonData);
						_logger.LogInfo("Received Event: MESSAGE_CREATE " + discordMessage.Content, ConsoleColor.Green);
						MessageCreated?.Invoke(discordMessage);
						break;
					case "GUILD_CREATE":
						var discordGuild = JsonConvert.DeserializeObject<DiscordGuild>(eventJsonData);
						_logger.LogInfo("Received Event: GUILD_CREATE " + discordGuild.Name, ConsoleColor.Green);
						GuildCreated?.Invoke(discordGuild);
						break;
					case "READY":
						var ready = JsonConvert.DeserializeObject<Ready>(eventJsonData);
						_logger.LogInfo("Received Event: READY Our username is " + ready.User.Username, ConsoleColor.Green);
						Ready?.Invoke(ready);
						break;
					case "TYPING_START":
						var typingStart = JsonConvert.DeserializeObject<TypingStart>(eventJsonData);
						_logger.LogInfo("Received Event: TYPING_START " + typingStart.UserId, ConsoleColor.Green);
						break;
					default:
						var ex = new ArgumentOutOfRangeException(nameof(eventName), eventName, "Unexpected Event Name");
						_logger.LogError(ex);
						throw ex;
				}
		}
	}
}
