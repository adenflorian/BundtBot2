using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Extensions;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway {
	public class DiscordGatewayClient {
		public delegate void OperationHandler(string eventName, object eventData);
		public event OperationHandler DispatchReceived;
		public event OperationHandler HeartbackAckReceived;
		public event OperationHandler HelloReceived;

		readonly ClientWebSocketWrapper _clientWebSocketWrapper = new ClientWebSocketWrapper();
		readonly string _authToken;

		int _lastSequenceReceived;

		public DiscordGatewayClient(string authToken) {
			_authToken = authToken;
			HelloReceived += OnHelloReceived;
			_clientWebSocketWrapper.MessageReceived += OnMessageReceived;
		}

		async void OnHelloReceived(string eventName, object eventData) {
			// TODO Drop the static MyLogger class for a normal class that gets passed into
			// other classes which can specify a prefix
			MyLogger.LogInfo("Gateway: Received Hello from Gateway", ConsoleColor.Green);
			var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
			StartHeartBeatLoop(hello.HeartbeatInterval);
			await SendGatewayIdentify();
		}

		public async Task ConnectAsync(Uri gatewayUrl) {
			var modifiedGatewayUrl = gatewayUrl.AddParameter("v", "5").AddParameter("encoding", "'json'");
			await _clientWebSocketWrapper.ConnectAsync(modifiedGatewayUrl);
			MyLogger.LogInfo($"Gateway: Connected to Gateway", ConsoleColor.Green);
		}

		public async Task SendGatewayIdentify() {
			MyLogger.LogInfo("Gateway: Sending GatewayIdentify to Gateway", ConsoleColor.Green);
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
				LargeThreshold = Threshold.Maximum,
			});
		}

		public void StartHeartBeatLoop(TimeSpan interval) {
			MyLogger.LogInfo("Gateway: Heartbeat loop started", ConsoleColor.Green);
			Task.Run(async () => {
				while (true) {
					await SendHeartBeat();
					Thread.Sleep(interval);
				}
			});
		}

		public async Task SendHeartBeat() {
			MyLogger.LogInfo("Gateway: Sending Heartbeat ♥ →");
			await SendOpCodeAsync(OpCode.Heartbeat, _lastSequenceReceived);
		}

		async Task SendOpCodeAsync(OpCode opCode, object eventData) {
			var gatewayPayload = new GatewayPayload(opCode, eventData);
			var jsonGatewayPayload = gatewayPayload.Serialize();

			MyLogger.LogInfo($"Gateway: Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
			MyLogger.LogDebug("Gateway: " + jsonGatewayPayload);

			await _clientWebSocketWrapper.SendAsync(jsonGatewayPayload);

			MyLogger.LogInfo($"Gateway: Sent {gatewayPayload.GatewayOpCode}");
		}

		public void StartReceiveLoop() {
			_clientWebSocketWrapper.StartReceiveLoop();
		}

		void OnMessageReceived(string message) {
			var gatewayPayload = JsonConvert.DeserializeObject<GatewayPayload>(message);

			StoreSequenceNumberForHeartbeat(gatewayPayload);

			MyLogger.LogInfo("Gateway: Message received from gateway (opcode: " +
				gatewayPayload.GatewayOpCode + ")");
			MyLogger.LogDebug("Gateway: " + message.Prettify());

			switch (gatewayPayload.GatewayOpCode) {
				case OpCode.Dispatch:
					Task.Run(() => DispatchReceived?.Invoke(gatewayPayload.EventName, gatewayPayload.EventData));
					break;
				case OpCode.HeartbackAck:
					HeartbackAckReceived?.Invoke(gatewayPayload.EventName, gatewayPayload.EventData);
					break;
				case OpCode.Hello:
					HelloReceived?.Invoke(gatewayPayload.EventName, gatewayPayload.EventData);
					break;
				default:
					MyLogger.LogWarning($"Received an OpCode with no handler: {gatewayPayload.GatewayOpCode}");
					break;
			}
		}

		void StoreSequenceNumberForHeartbeat(GatewayPayload receivedGatewayDispatch) {
			if (receivedGatewayDispatch.SequenceNumber.HasValue) {
				_lastSequenceReceived = receivedGatewayDispatch.SequenceNumber.Value;
			}
		}
	}
}
