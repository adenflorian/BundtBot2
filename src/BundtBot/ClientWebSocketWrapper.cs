using System;
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

		public async Task ConnectAsync(Uri serverUri)
		{
			await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
			MyLogger.LogInfo($"Gateway: Connected to {serverUri} (ClientWebSocket State: {_clientWebSocket.State})",
				ConsoleColor.Green);
		}

		public async Task SendAsync(string data) {
			var bytes = _utf8Encoding.GetBytes(data);
			var sendBuffer = new ArraySegment<byte>(bytes);

			await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);

			MyLogger.LogInfo($"Gateway: Sent (ClientWebSocket State: {_clientWebSocket.State})");
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
			MessageReceived?.Invoke(message);
		}
	}
}
