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
			LogConnected(_logger, _serverUri, _clientWebSocket);
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
            LogReconnected(_logger, _serverUri, _clientWebSocket);
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

		void StartSendLoop()
		{
			Task.Run(async () => {
				while (true) {
					await WaitForSomethingToBeInQueue();

					var message = _outgoingQueue.Dequeue();
					Debug.Assert(message != null);

					await Try.ForeverAsync(async () => await TrySendAsync(message.Item1), TimeEx._1second);
					
					message.Item2.Invoke();
				}
			});
		}

        async Task WaitForSomethingToBeInQueue()
        {
            while (_outgoingQueue.Count == 0) await Task.Delay(100);
        }

        async Task<bool> TrySendAsync(string messageToSend)
        {
            try
            {
                _logger.LogDebug($"Sending message... ({messageToSend.GetHashCode()})");
                await _clientWebSocket.SendAsync(CreateSendBuffer(messageToSend), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogDebug($"Sent! ({messageToSend.GetHashCode()})");
                return true;
            }
            catch (Exception ex)
            {
                if (_isDisposing) return true;
                _logger.LogError(ex);
                return false;
            }
        }

		ArraySegment<byte> CreateSendBuffer(string data)
		{
			var bytes = new UTF8Encoding().GetBytes(data);
			return new ArraySegment<byte>(bytes);
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
    }
}
