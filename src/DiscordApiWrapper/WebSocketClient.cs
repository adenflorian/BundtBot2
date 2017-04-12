using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordApiWrapper.Gateway;
using Newtonsoft.Json;

namespace BundtBot
{
    public class WebSocketClient
	{
		public delegate void MessageReceivedHandler();
		public event MessageReceivedHandler MessageReceived;

		public Queue<string> ReceivedMessages = new Queue<string>();

		readonly MyLogger _logger;
		readonly UTF8Encoding _utf8Encoding = new UTF8Encoding();
		readonly Queue<Tuple<string, Action>> _outgoingQueue = new Queue<Tuple<string, Action>>();
		readonly Uri _serverUri;
		
		ClientWebSocket _clientWebSocket = new ClientWebSocket();

		public WebSocketClient(Uri serverUri, string logPrefix, ConsoleColor prefixColor)
		{
			_serverUri = serverUri;
			_logger = new MyLogger(logPrefix + nameof(WebSocketClient), prefixColor);
		}

		public async Task ConnectAsync()
		{
			await DoConnectLoopAsync();

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
			await DoConnectLoopAsync();
			
            _logger.LogInfo(
                new LogMessage($"Reconnected to "),
                new LogMessage($"{_serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{_clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
        }

        async Task DoConnectLoopAsync()
        {
            while (true)
            {
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
                    _logger.LogError("[Connect Loop] Error while connecting websocket");
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
				while (true) {
					while (_outgoingQueue.Count == 0)
					{
						await Task.Delay(100);
					}

					var message = _outgoingQueue.Dequeue();

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
                            _logger.LogError($"[Send Loop] Error while sending message on websocket ({message.Item1.GetHashCode()})");
                            _logger.LogError(ex);

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
			var bytes = _utf8Encoding.GetBytes(data);
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
						_logger.LogError("[Receive Loop] Exception caught in ClientWebSocketWrapper ReceiveLoop.");
                        _logger.LogError(ex);
                        _logger.LogWarning($"[Receive Loop] _clientWebSocket.State: {_clientWebSocket.State.ToString()}");
                        _logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatus: {_clientWebSocket.CloseStatus.ToString()}");
                        _logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatusDescription: {_clientWebSocket.CloseStatusDescription}");

						message = "";

						_logger.LogWarning("[Receive Loop] Reconnecting ClientWebSocketWrapper.");

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
			ReceivedMessages.Enqueue(message);
			MessageReceived?.Invoke();
		}

        async Task OnCloseReceivedAsync(WebSocketReceiveResult result)
        {
			var codeString = result.CloseStatus.Value.ToString();

			string logMessage;

			if (CloseCodes.Codes.ContainsKey(codeString))
			{
				logMessage = "Received a message from Gateway with Close Status, will reconnect: " + CloseCodes.Codes[codeString];
			}
			else
			{
                logMessage = "Received a message from Gateway with Close Status, will reconnect: " + codeString;
			}
			
			if (result.CloseStatus.Value.ToString() == "4001")
			{
                _logger.LogCritical(logMessage);
			}
			else
			{
            	_logger.LogError(logMessage);
			}

            await ReconnectAsync();
        }
    }
}
