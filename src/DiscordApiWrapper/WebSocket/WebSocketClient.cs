using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtCommon;
using Newtonsoft.Json;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient : IDisposable
	{
		public event Action<string> MessageReceived;

		readonly MyLogger _logger;
		readonly Uri _serverUri;
		readonly Queue<Tuple<string, Action>> _outgoingQueue = new Queue<Tuple<string, Action>>();
		
		ClientWebSocket _clientWebSocket = new ClientWebSocket();
        bool _isDisposed;
		bool _isDisposing;

        public WebSocketClient(Uri serverUri, string logPrefix, ConsoleColor prefixColor)
		{
			_serverUri = serverUri;
			_logger = new MyLogger(logPrefix + nameof(WebSocketClient), prefixColor);
		}

        ~WebSocketClient() => Dispose();

        public void Dispose()
        {
			_isDisposing = true;
            if (_isDisposed == false)
            {
                _logger.LogDebug("Disposing");
                _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None).Wait();
                _clientWebSocket.Dispose();
                _isDisposed = true;
            }
        }

        public async Task ConnectAsync()
		{
			await DoConnectLoopAsync();
			LogConnected(_serverUri, _clientWebSocket);
			StartReceiveLoop();
			StartSendLoop();
		}

        public async Task SendMessageUsingQueueAsync(string data)
        {
            if (data == null) throw new ArgumentException();

			var isDone = false;
			_logger.LogDebug($"Enqueueing {data.Substring(0, Math.Min(data.Length, 20))}...");
			_outgoingQueue.Enqueue(Tuple.Create<string, Action>(data, () => { isDone = true; }));
			while (isDone == false)
			{
				if (_isDisposing) throw new OperationCanceledException();
				await Task.Delay(10);
			}
        }

        async Task ReconnectAsync()
        {
			await DoConnectLoopAsync();
            LogReconnected(_serverUri, _clientWebSocket);
        }

        async Task DoConnectLoopAsync()
        {
            while (true)
            {
                try
                {
                    _logger.LogInfo($"Connecting websocket to {_serverUri}");
                    await _clientWebSocket.ConnectAsync(_serverUri, new CancellationTokenSource(TimeEx._5seconds).Token);
                    break;
                }
                catch (System.Exception ex)
                {
                    if (_isDisposing) return;
                    _logger.LogError(ex, true);
					await Wait.AndLogAsync(TimeEx._5seconds, _logger);
                    _clientWebSocket.Dispose();
                    _clientWebSocket = new ClientWebSocket();
                }
            }
        }
    }
}
