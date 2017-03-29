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

		static readonly MyLogger _logger = new MyLogger(nameof(ClientWebSocketWrapper), ConsoleColor.DarkCyan);
		readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();
		readonly UTF8Encoding _utf8Encoding = new UTF8Encoding();
		readonly Queue<Tuple<string, Action>> _outgoingQueue = new Queue<Tuple<string, Action>>();
		readonly Uri _serverUri;

		public ClientWebSocketWrapper(Uri serverUri)
		{
			_serverUri = serverUri;
		}

		public async Task ConnectAsync()
		{
			await _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
			_logger.LogInfo(
				new LogMessage($"Connected to "),
				new LogMessage($"{_serverUri}", ConsoleColor.Cyan),
				new LogMessage($" (ClientWebSocket State: "),
				new LogMessage($"{_clientWebSocket.State}", ConsoleColor.Green),
				new LogMessage($")"));
			StartReceiveLoop();
			StartSendLoop();
		}

        async Task ReconnectAsync()
        {
            await _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
            _logger.LogInfo(
                new LogMessage($"Reconnected to "),
                new LogMessage($"{_serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{_clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
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
			var waitTimeMs = 1000;
			Task.Run(async () => {
				var message = "";
				while (_clientWebSocket.State == WebSocketState.Open) {
					try
					{
						var result = await ReceiveAsync();

						_logger.LogTrace(JsonConvert.SerializeObject(result.Item1, Formatting.Indented));

						message += result.Item2;

						if (result.Item1.EndOfMessage == false) continue;

						OnMessageReceived(message);
						message = "";
					}
					catch (Exception ex)
					{
						_logger.LogWarning("Exception caught in ClientWebSocketWrapper ReceiveLoop.");

                        if (waitTimeMs > 1000 * 60)
                        {
                            _logger.LogCritical(ex);
                            throw;
                        }

						_logger.LogError(ex);

						_logger.LogWarning($"Waiting for {waitTimeMs / 1000} seconds, then reconnecting");
						await Task.Delay(waitTimeMs);
						waitTimeMs *= 2;
						_logger.LogWarning($"Doubled web socket reconnect wait time to {waitTimeMs / 1000} seconds");
						message = "";

						_logger.LogWarning("Reconnecting ClientWebSocketWrapper.");

						await ReconnectAsync();
					}
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
