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
			StartReceiveLoop();
			StartSendLoop();
		}

		public async Task SendMessageUsingQueueAsync(string data)
		{
			await Task.Run(async () => {
				var isDone = false;
				_outgoingQueue.Enqueue(Tuple.Create<string, Action>(data, () => {
					isDone = true;
				}));
				while (isDone == false) {
					await Task.Delay(10);
				}
			});
		}

		void StartSendLoop()
		{
			Task.Run(async () => {
				while (_clientWebSocket.State == WebSocketState.Open) {
					while (_outgoingQueue.Count == 0) {
						await Task.Delay(100);
					}
					var message = _outgoingQueue.Dequeue();
					await SendAsync(message.Item1);
					message.Item2.Invoke();
				}
			});
		}

		async Task SendAsync(string data)
		{
			var sendBuffer = CreateSendBuffer(data);

			await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);

			_logger.LogDebug($"Sent {sendBuffer.Count} bytes (ClientWebSocket State: {_clientWebSocket.State})");
		}

		ArraySegment<byte> CreateSendBuffer(string data)
		{
			var bytes = _utf8Encoding.GetBytes(data);
			var sendBuffer = new ArraySegment<byte>(bytes);
			return sendBuffer;
		}

		void StartReceiveLoop()
		{
			Task.Run(async () => {
				var message = "";
				while (_clientWebSocket.State == WebSocketState.Open) {
					var result = await ReceiveAsync();

					_logger.LogDebug(JsonConvert.SerializeObject(result.Item1, Formatting.Indented));

					message += result.Item2;

					if (result.Item1.EndOfMessage == false) continue;

					OnMessageReceived(message);
					message = "";
				}
			});
		}

		async Task<Tuple<WebSocketReceiveResult, string>> ReceiveAsync()
		{
			var receiveBuffer = CreateReceiveBuffer();
			var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

			_logger.LogDebug($"Received {receiveResult.Count} bytes on ClientWebSocket" +
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
