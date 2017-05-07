using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtBot.Extensions;
using BundtCommon;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient
    {
        struct OutgoingMessage
        {
            public string Content;
            public Action Callback;
        }

        void StartSendLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await GetNextOutgoingMessageAsync();

                    await Try.ForeverAsync(async () => await TrySendAsync(message.Content), TimeEx._1second);

                    message.Callback.Invoke();
                }
            });
        }

        async Task<OutgoingMessage> GetNextOutgoingMessageAsync()
        {
            while (_outgoingQueue.Count == 0) await Task.Delay(100);
            var message = _outgoingQueue.Dequeue();
            return message;
        }

        async Task<bool> TrySendAsync(string messageToSend)
        {
            try
            {
                await SendAsync(messageToSend, _clientWebSocket);
                return true;
            }
            catch (Exception ex)
            {
                if (_isDisposing) return true;
                _logger.LogError(ex);
                return false;
            }
        }

        async Task SendAsync(string messageToSend, ClientWebSocket _clientWebSocket)
        {
            Debug.Assert(messageToSend != null);
            _logger.LogDebug($"Sending message... ({messageToSend.GetHashAsLowercaseHex()})");
            await _clientWebSocket.SendAsync(CreateSendBuffer(messageToSend), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogDebug($"Sent! ({messageToSend.GetHashAsLowercaseHex()})");
        }

        ArraySegment<byte> CreateSendBuffer(string data)
        {
            var bytes = new UTF8Encoding().GetBytes(data);
            return new ArraySegment<byte>(bytes);
        }
    }
}