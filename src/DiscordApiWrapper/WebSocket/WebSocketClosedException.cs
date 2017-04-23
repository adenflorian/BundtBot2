using System;
using System.Net.WebSockets;
using DiscordApiWrapper.Gateway;

namespace DiscordApiWrapper.WebSocket
{
    public class WebSocketClosedException : Exception
    {
        public readonly WebSocketCloseStatus CloseStatus;

        public WebSocketClosedException(WebSocketCloseStatus closeStatus)
        {
            CloseStatus = closeStatus;
        }
    }
}