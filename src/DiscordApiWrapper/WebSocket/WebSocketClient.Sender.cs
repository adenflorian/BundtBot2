using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BundtCommon;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient
    {
        void StartSendLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
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
    }
}