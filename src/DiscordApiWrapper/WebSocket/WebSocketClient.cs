using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot;
using Newtonsoft.Json;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient : IDisposable
	{
		public event Action<string> MessageReceived;

		readonly MyLogger _logger;
		readonly Queue<Tuple<string, Action>> _outgoingQueue = new Queue<Tuple<string, Action>>();
		readonly Uri _serverUri;
		
		ClientWebSocket _clientWebSocket = new ClientWebSocket();
        bool _isDisposed;
		bool _isDisposing;

        public WebSocketClient(Uri serverUri, string logPrefix, ConsoleColor prefixColor)
		{
			_serverUri = serverUri;
			_logger = new MyLogger(logPrefix + nameof(WebSocketClient), prefixColor);
		}

        public async Task ConnectAsync()
		{
			await DoConnectLoopAsync();

			LogConnected(_logger, _serverUri, _clientWebSocket);

			StartReceiveLoop();
			StartSendLoop();
		}

        async Task ReconnectAsync()
        {
			await DoConnectLoopAsync();

            LogReconnected(_logger, _serverUri, _clientWebSocket);
        }

        async Task DoConnectLoopAsync()
        {
            while (true)
            {
                if (_isDisposing) return;
                try
                {
                    _clientWebSocket.Dispose();
                    _clientWebSocket = new ClientWebSocket();
                    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    _logger.LogInfo("[Connect Loop] Connecting websocket... (" + _serverUri + ")");
                    await _clientWebSocket.ConnectAsync(_serverUri, tokenSource.Token);
                    break;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, true);
					var waitAmount = TimeSpan.FromSeconds(5);
                    _logger.LogWarning($"[Connect Loop] Waiting {waitAmount.TotalSeconds} seconds before attempting to reconnect...");
					await Task.Delay(waitAmount);
                }
            }
        }

        public async Task SendMessageUsingQueueAsync(string data)
		{
			if (data == null) throw new ArgumentException();

			await Task.Run(async () => {
				var isDone = false;
				_logger.LogDebug($"Enqueueing {data.Substring(0, Math.Min(data.Length, 20))}...");
				_outgoingQueue.Enqueue(Tuple.Create<string, Action>(data, () => {
					isDone = true;
				}));
				while (isDone == false) {
                    if (_isDisposing) return;
					await Task.Delay(10);
				}
			});
		}

		void StartSendLoop()
		{
			Task.Run(async () => {
				while (true) {
					while (_outgoingQueue.Count == 0) await Task.Delay(100);

                    // FIXME How is message ever null here?
                    // Was null enqueued?
                    Debug.Assert(_outgoingQueue.Count > 0);
					var message = _outgoingQueue.Dequeue();
					Debug.Assert(message != null);

					while (true)
					{
						try
						{
                            _logger.LogInfo($"[Send Loop] Sending message on websocket... ({message.Item1.GetHashCode()})");
							await SendAsync(message.Item1);
                            _logger.LogInfo($"[Send Loop] Sent! ({message.Item1.GetHashCode()})");
							break;
						}
						catch (System.Exception ex)
						{
                            if (_isDisposing) return;
                            _logger.LogError($"[Send Loop] Error while sending message on websocket ({message.Item1.GetHashCode()})");
                            _logger.LogError(ex);

							// TODO Refactor
							while (true)
							{
								if (_clientWebSocket.State == WebSocketState.Open) break;
								var secondsToWait = 5;
                                _logger.LogWarning($"[Send Loop] WebSocket is not Open, waiting for {secondsToWait} seconds then checking again... (State: {_clientWebSocket.State})");
								await Task.Delay(TimeSpan.FromSeconds(secondsToWait));
							}

                            _logger.LogWarning($"[Send Loop] Waiting for 1 second then retrying send... ({message.Item1.GetHashCode()})");
							await Task.Delay(TimeSpan.FromSeconds(1));
						}
					}
					
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
			var bytes = new UTF8Encoding().GetBytes(data);
			var sendBuffer = new ArraySegment<byte>(bytes);
			return sendBuffer;
		}

		void StartReceiveLoop()
		{
			Task.Run(async () => {
				var message = "";
				while (_clientWebSocket.State == WebSocketState.Open) {
					try
					{
						var result = await ReceiveAsync();

                        _logger.LogTrace(JsonConvert.SerializeObject(result.Item1, Formatting.Indented));
                        _logger.LogTrace(result.Item2);

						message += result.Item2;

						if (result.Item1.EndOfMessage == false)
						{
							continue;
						}
						else if (result.Item1.CloseStatus.HasValue)
						{
                            await OnCloseReceivedAsync(result.Item1);
						}
						else if (message.Length == 0)
                        {
                            _logger.LogWarning("Received 0 length message from Gateway, will not invoke MessageReceived");
                        }
						else
						{
							OnMessageReceived(message);
						}
						
						message = "";
					}
					catch (Exception ex)
					{
						if (_isDisposing) return;
						LogReceiveLoopException(_logger, ex, _clientWebSocket);
						message = "";
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

			var receivedString = new UTF8Encoding().GetString(receiveBuffer.Array, 0, receiveResult.Count);

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

        async Task OnCloseReceivedAsync(WebSocketReceiveResult result)
        {
			var codeString = result.CloseStatus.Value.ToString();

			LogCloseReceived(_logger, codeString);

            await ReconnectAsync();
        }

        ~WebSocketClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposing = true;
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing");
					_clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None).Wait();
					_clientWebSocket.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
