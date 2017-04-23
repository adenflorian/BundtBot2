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
        struct ReceivedMessage
        {
            public String Content;
            public bool IsCompleted;
        }

        struct ReceivedData
        {
            public WebSocketReceiveResult Result;
            public string Data;
            public bool IsCloseRequested => Result.CloseStatus.HasValue;
        }

        void StartReceiveLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await ReceiveMessageAsync();
                    }
                    catch (Exception ex)
                    {
                        if (_isDisposing) return;
                        LogReceiveLoopException(ex);
                        await ReconnectAsync();
                    }
                }
            });
        }

        async Task ReceiveMessageAsync()
        {
            var completedMessage = new ReceivedMessage();
            ReceivedData receivedData;

            while (completedMessage.IsCompleted == false)
            {
                receivedData = await ReceiveAsync(_clientWebSocket);

                ThrowIfCloseStatus(receivedData.Result.CloseStatus);

                completedMessage.Content += receivedData.Data;
                completedMessage.IsCompleted = receivedData.Result.EndOfMessage;
            }
            
            OnMessageReceived(completedMessage.Content);
        }

        async Task<ReceivedData> ReceiveAsync(ClientWebSocket _clientWebSocket)
        {
            // 8192 is an arbitrary number
            var receiveBuffer = new ArraySegment<byte>(new byte[8192]);

            var receiveResult = await _clientWebSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

            var receivedString = new UTF8Encoding().GetString(receiveBuffer.Array, 0, receiveResult.Count);

            LogReceived(receiveResult, receivedString);

            return new ReceivedData { Result = receiveResult, Data = receivedString };
        }

        void ThrowIfCloseStatus(WebSocketCloseStatus? closeStatus)
        {
            if (closeStatus.HasValue)
            {
                LogCloseReceived(closeStatus.Value.ToString());
                throw new WebSocketClosedException(closeStatus.Value);
            }
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