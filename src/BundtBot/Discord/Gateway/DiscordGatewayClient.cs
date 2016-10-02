using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Extensions;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway {
	// TODO Extract methods that use _clientWebSocket into new class
	public class DiscordGatewayClient {
		public delegate void OperationHandler(string eventName, object eventData);
		public event OperationHandler DispatchReceived;
		public event OperationHandler HeartbackAckReceived;
		public event OperationHandler HelloReceived;

		readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();
		readonly UTF8Encoding _utf8Encoding = new UTF8Encoding();
		readonly string _authToken;

		int _lastSequenceReceived;

		public DiscordGatewayClient(string authToken) {
			_authToken = authToken;
			HelloReceived += OnHelloReceived;
		}

		async void OnHelloReceived(string eventName, object eventData) {
			// TODO Drop the static MyLogger class for a normal class that gets passed into
			// other classes which can specify a prefix
			MyLogger.LogInfo("Gateway: Received Hello from Discord Gateway", ConsoleColor.Green);
			var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
			await SendHeartBeat();
			StartHeartBeatLoop(hello.HeartbeatInterval);
			await SendGatewayIdentify();
		}

		public async Task Connect(Uri gatewayUrl) {
			var modifiedGatewayUrl = gatewayUrl.AddParameter("v", "5").AddParameter("encoding", "'json'");
			await _clientWebSocket.ConnectAsync(modifiedGatewayUrl, CancellationToken.None);
			MyLogger.LogInfo($"Gateway: Connected to Gateway (state: {_clientWebSocket.State})", ConsoleColor.Green);
		}

		public async Task SendGatewayIdentify() {
			await SendAsync(OpCode.Identify, new GatewayIdentify {
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
			Task.Run(async () => {
				while (true) {
					Thread.Sleep(interval);
					await SendHeartBeat();
				}
			});
		}

		public async Task SendHeartBeat() {
			MyLogger.LogInfo("Gateway: Sending Heartbeat ♥ →");
			await SendAsync(OpCode.Heartbeat, _lastSequenceReceived);
		}

		async Task SendAsync(OpCode opCode, object eventData) {
			var gatewayPayload = new GatewayPayload(opCode, eventData);
			var jsonGatewayDispatch = gatewayPayload.Serialize();

			MyLogger.LogInfo($"Gateway: Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
			MyLogger.LogDebug("Gateway: " + jsonGatewayDispatch);

			var bytes = _utf8Encoding.GetBytes(jsonGatewayDispatch);
			var sendBuffer = new ArraySegment<byte>(bytes);
			await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);

			MyLogger.LogInfo($"Gateway: Sent (socket-state: {_clientWebSocket.State})");
		}

		public void StartReceiveLoop() {
			Task.Run(async () => {
				await ReceiveLoop();
			});
		}

		async Task ReceiveLoop() {
			var message = "";
			while (_clientWebSocket.State == WebSocketState.Open) {
				var result = await ReceiveAsync();

				MyLogger.LogDebug("Gateway: Received bytes on ClientWebSocket\n" +
				                  JsonConvert.SerializeObject(result.Item1, Formatting.Indented));

				message += result.Item2;

				if (result.Item1.EndOfMessage == false) continue;

				OnMessageReceived(message);
				message = "";
			}
		}

		async Task<Tuple<WebSocketReceiveResult, string>> ReceiveAsync() {
			const int arbitraryBufferSize = 8192;
			var receiveBuffer = new ArraySegment<byte>(new byte[arbitraryBufferSize]);
			var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
			var msg = _utf8Encoding.GetString(receiveBuffer.Array, 0, receiveResult.Count);
			return Tuple.Create(receiveResult, msg);
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
