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
                var message = "";
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var receivedData = await ReceiveAsync();

                        message += receivedData.Data;

                        if (receivedData.Result.EndOfMessage == false) continue;

                        if (receivedData.Result.CloseStatus.HasValue)
                        {
                            await OnCloseReceivedAsync(receivedData.Result);
                            message = "";
                            continue;
                        }

                        OnMessageReceived(message);
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

        async Task<ReceivedData> ReceiveAsync()
        {
            var receiveBuffer = CreateReceiveBuffer();

            var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
            LogReceiveResult(receiveResult);

            var receivedString = new UTF8Encoding().GetString(receiveBuffer.Array, 0, receiveResult.Count);
            _logger.LogTrace(receivedString);

            return new ReceivedData(receiveResult, receivedString);
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

        void LogReceiveResult(WebSocketReceiveResult result)
        {
            _logger.LogDebug($"Received {result.Count} bytes on ClientWebSocket" +
                                $"(EndOfMessage: {result.EndOfMessage})");
            _logger.LogTrace(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        static ArraySegment<byte> CreateReceiveBuffer()
        {
            const int arbitraryBufferSize = 8192;
            return new ArraySegment<byte>(new byte[arbitraryBufferSize]);
        }

        async Task OnCloseReceivedAsync(WebSocketReceiveResult result)
        {
            var codeString = result.CloseStatus.Value.ToString();

            LogCloseReceived(_logger, codeString);

            await ReconnectAsync();
        }
    }
}