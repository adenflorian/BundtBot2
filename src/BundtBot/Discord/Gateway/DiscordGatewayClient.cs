using System;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Discord.Gateway.Models;
using BundtBot.Discord.Gateway.Operation;
using BundtBot.Extensions;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway
{
	public class DiscordGatewayClient
	{
		public delegate void OperationHandler(string eventName, object eventData);
		public event OperationHandler DispatchReceived;
		public event OperationHandler HeartbackAckReceived;
		public event OperationHandler HelloReceived;

		readonly ClientWebSocketWrapper _clientWebSocketWrapper = new ClientWebSocketWrapper();
		readonly MyLogger _logger = new MyLogger(nameof(DiscordGatewayClient));
		readonly string _authToken;

		int _lastSequenceReceived;

		public DiscordGatewayClient(string authToken)
		{
			_authToken = authToken;
			HelloReceived += OnHelloReceived;
			_clientWebSocketWrapper.MessageReceived += OnMessageReceived;
		}

		async void OnHelloReceived(string eventName, object eventData)
		{
			_logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
			var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());
			StartHeartBeatLoop(hello.HeartbeatInterval);
			await SendGatewayIdentify();
		}

		public async Task ConnectAsync(Uri gatewayUrl)
		{
			var modifiedGatewayUrl = gatewayUrl.AddParameter("v", "5").AddParameter("encoding", "'json'");
			await _clientWebSocketWrapper.ConnectAsync(modifiedGatewayUrl);
			_logger.LogInfo($"Connected to Gateway", ConsoleColor.Green);
		}

		public async Task SendGatewayIdentify()
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
				LargeThreshold = Threshold.Maximum,
			});
		}

		public void StartHeartBeatLoop(TimeSpan interval)
		{
			_logger.LogInfo("Heartbeat loop started", ConsoleColor.Green);
			Task.Run(async () => {
				while (true) {
					await SendHeartBeat();
					Thread.Sleep(interval);
				}
			});
		}

		public async Task SendHeartBeat()
		{
			_logger.LogInfo("Sending Heartbeat ♥ →");
			await SendOpCodeAsync(OpCode.Heartbeat, _lastSequenceReceived);
		}

		async Task SendOpCodeAsync(OpCode opCode, object eventData)
		{
			var gatewayPayload = new GatewayPayload(opCode, eventData);
			var jsonGatewayPayload = gatewayPayload.Serialize();

			_logger.LogInfo($"Sending opcode {gatewayPayload.GatewayOpCode} to gateway...");
			_logger.LogDebug("" + jsonGatewayPayload);

			await _clientWebSocketWrapper.SendAsync(jsonGatewayPayload);

			_logger.LogInfo($"Sent {gatewayPayload.GatewayOpCode}");
		}

		public void StartReceiveLoop()
		{
			_clientWebSocketWrapper.StartReceiving();
		}

		void OnMessageReceived(string message)
		{
			var gatewayPayload = JsonConvert.DeserializeObject<GatewayPayload>(message);

			StoreSequenceNumberForHeartbeat(gatewayPayload);

			_logger.LogInfo("Message received from gateway (opcode: " +
				gatewayPayload.GatewayOpCode + ")");
			_logger.LogDebug("" + message.Prettify());

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
					_logger.LogWarning($"Received an OpCode with no handler: {gatewayPayload.GatewayOpCode}");
					break;
			}
		}

		void StoreSequenceNumberForHeartbeat(GatewayPayload receivedGatewayDispatch)
		{
			if (receivedGatewayDispatch.SequenceNumber.HasValue) {
				_lastSequenceReceived = receivedGatewayDispatch.SequenceNumber.Value;
			}
		}
	}
}
