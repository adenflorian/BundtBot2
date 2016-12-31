using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BundtBot
{
	public class ClientWebSocketWrapper
	{
		public delegate void MessageReceivedHandler(string message);
		public event MessageReceivedHandler MessageReceived;

		readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();
		readonly UTF8Encoding _utf8Encoding = new UTF8Encoding();
		readonly MyLogger _logger = new MyLogger(nameof(ClientWebSocketWrapper));

		readonly Queue<Tuple<string, Action>> _outgoingQueue = new Queue<Tuple<string, Action>>();

		public async Task ConnectAsync(Uri serverUri)
		{
			await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
			_logger.LogInfo($"Connected to {serverUri} (ClientWebSocket State: {_clientWebSocket.State})",
				ConsoleColor.Green);
			StartSending();
		}

		public async Task SendQueuedAsync(string data)
		{
			await Task.Run(() => {
				var isDone = false;
				_outgoingQueue.Enqueue(Tuple.Create<string, Action>(data, () => {
					isDone = true;
				}));
				while (isDone == false) {
					Thread.Sleep(10);
				}
			});
		}

		void StartSending()
		{
			Task.Run(async () => {
				await SendLoop();
			});
		}

		async Task SendLoop()
		{
			while (_clientWebSocket.State == WebSocketState.Open) {
				while (_outgoingQueue.Count == 0) {
					Thread.Sleep(50);
				}
				var msg = _outgoingQueue.Dequeue();
				await SendAsync(msg.Item1);
				msg.Item2.Invoke();
			}
		}

		async Task SendAsync(string data)
		{
			var sendBuffer = CreateSendBuffer(data);

			await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);

			_logger.LogInfo($"Sent {sendBuffer.Count} bytes (ClientWebSocket State: {_clientWebSocket.State})");
		}

		ArraySegment<byte> CreateSendBuffer(string data)
		{
			var bytes = _utf8Encoding.GetBytes(data);
			var sendBuffer = new ArraySegment<byte>(bytes);
			return sendBuffer;
		}

		public void StartReceiving()
		{
			Task.Run(async () => {
				await ReceiveLoop();
			});
		}

		async Task ReceiveLoop()
		{
			var message = "";
			while (_clientWebSocket.State == WebSocketState.Open) {
				var result = await ReceiveAsync();
				
				_logger.LogDebug(JsonConvert.SerializeObject(result.Item1, Formatting.Indented));

				message += result.Item2;

				if (result.Item1.EndOfMessage == false) continue;

				OnMessageReceived(message);
				message = "";
			}
		}

		async Task<Tuple<WebSocketReceiveResult, string>> ReceiveAsync()
		{
			var receiveBuffer = CreateReceiveBuffer();
			var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

			_logger.LogInfo($"Received {receiveResult.Count} bytes on ClientWebSocket" +
			                $"(EndOfMessage: {receiveResult.EndOfMessage})");

			var receivedString = _utf8Encoding.GetString(receiveBuffer.Array, 0, receiveResult.Count);

			return Tuple.Create(receiveResult, receivedString);
		}

		static ArraySegment<byte> CreateReceiveBuffer()
		{
			const int arbitraryBufferSize = 8192;
			return new ArraySegment<byte>(new byte[arbitraryBufferSize]);
		}

		void OnMessageReceived(string message)
		{
			MessageReceived?.Invoke(message);
		}
	}
}
