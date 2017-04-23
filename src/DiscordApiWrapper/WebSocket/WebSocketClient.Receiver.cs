using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient
    {
        struct ReceivedData
        {
            public readonly WebSocketReceiveResult Result;
            public readonly string Data;

            public bool IsCloseRequested => Result.CloseStatus.HasValue;

            public ReceivedData(WebSocketReceiveResult result, string data)
            {
                Result = result;
                Data = data;
            }
        }
        
        void StartReceiveLoop()
        {
            Task.Run(async () =>
            {
                var completedMessage = "";
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var receivedData = await ReceiveAsync();

                        if (receivedData.IsCloseRequested)
                        {
                            LogCloseReceived(receivedData.Result.CloseStatus.Value.ToString());
                            await ReconnectAsync();
                            completedMessage = "";
                            continue;
                        }

                        completedMessage += receivedData.Data;

                        if (receivedData.Result.EndOfMessage)
                        {
                            OnMessageReceived(completedMessage);
                            completedMessage = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_isDisposing) return;
                        LogReceiveLoopException(ex, _clientWebSocket);
                        completedMessage = "";
                        await ReconnectAsync();
                    }
                }
            });
        }

        async Task<ReceivedData> ReceiveAsync()
        {
            var receiveBuffer = CreateReceiveBuffer();

            var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
            LogReceiveResult(receiveResult);

            var receivedString = new UTF8Encoding().GetString(receiveBuffer.Array, 0, receiveResult.Count);
            _logger.LogTrace(receivedString);

            return new ReceivedData(receiveResult, receivedString);
        }

        static ArraySegment<byte> CreateReceiveBuffer()
        {
            const int arbitraryBufferSize = 8192;
            return new ArraySegment<byte>(new byte[arbitraryBufferSize]);
        }

        void OnMessageReceived(string message)
        {
            if (message.Length == 0)
            {
                _logger.LogWarning("Received 0 length message from Gateway, will not invoke MessageReceived");
            }
            else
            {
                MessageReceived?.Invoke(message);
            }
        }
    }
}