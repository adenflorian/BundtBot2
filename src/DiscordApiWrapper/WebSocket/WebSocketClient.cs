using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using BundtCommon;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient : IDisposable
	{
		public event Action<string> MessageReceived;

		readonly MyLogger _logger;
		readonly Uri _serverUri;
		readonly Queue<OutgoingMessage> _outgoingQueue = new Queue<OutgoingMessage>();
		
		ClientWebSocket _clientWebSocket = new ClientWebSocket();
        bool _isDisposed;
		bool _isDisposing;

        public WebSocketClient(Uri serverUri, string logPrefix, ConsoleColor prefixColor)
		{
			_serverUri = serverUri;
			_logger = new MyLogger(logPrefix + nameof(WebSocketClient), prefixColor);
            _logger.SetLogLevel(BundtFig.GetValue("loglevel-websocketclient"));
		}

        public async Task ConnectAsync()
		{
			await DoConnectLoopAsync();
			LogConnected(_serverUri);
			StartReceiveLoop();
			StartSendLoop();
		}

        public async Task SendMessageUsingQueueAsync(string data)
        {
            if (data == null) throw new ArgumentException();

			var isDone = false;
			_logger.LogDebug($"Enqueueing {data.Substring(0, Math.Min(data.Length, 20))}...");
			_outgoingQueue.Enqueue(new OutgoingMessage{Content = data, Callback = () => { isDone = true; }});
			while (isDone == false)
			{
				if (_isDisposing) throw new OperationCanceledException();
				await Task.Delay(10);
			}
        }

        async Task ReconnectAsync()
        {
			await DoConnectLoopAsync();
            LogReconnected(_serverUri);
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
    }
}
