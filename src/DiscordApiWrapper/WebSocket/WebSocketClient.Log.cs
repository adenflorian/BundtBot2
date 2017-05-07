using System;
using System.Net.WebSockets;
using BundtBot;
using DiscordApiWrapper.Gateway;
using Newtonsoft.Json;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient
    {
        void LogConnected(Uri serverUri)
        {
            _logger.LogInfo(
                new LogMessage($"Connected to "),
                new LogMessage($"{serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{_clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
        }

        void LogReconnected(Uri serverUri)
        {
            _logger.LogInfo(
                new LogMessage($"Reconnected to "),
                new LogMessage($"{serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{_clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
        }

        void LogReceiveLoopException(Exception ex)
        {
            _logger.LogError("[Receive Loop] Exception caught in ReceiveLoop.");
            _logger.LogError(ex);

            _logger.LogWarning($"[Receive Loop] _clientWebSocket.State: {_clientWebSocket.State.ToString()}");
            _logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatus: {_clientWebSocket.CloseStatus.ToString()}");
            _logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatusDescription: {_clientWebSocket.CloseStatusDescription}");

            _logger.LogWarning("[Receive Loop] Reconnecting.");
        }

        void LogCloseReceived(string codeString)
        {
            string logMessage = "Received a message from Gateway with Close Status: ";

            if (CloseCodes.Codes.ContainsKey(codeString))
            {
                logMessage += CloseCodes.Codes[codeString];
            }
            else
            {
                logMessage += codeString;
            }

            if (codeString == "4001")
            {
                _logger.LogCritical(logMessage);
            }
            else
            {
                if (_isDisposing)
                {
                    _logger.LogInfo(logMessage);
                }
                else
                {
                    _logger.LogError(logMessage);
                }
            }
        }

        void LogReceived(WebSocketReceiveResult result, string receivedString)
        {
            _logger.LogDebug($"Received {result.Count} bytes (EndOfMessage: {result.EndOfMessage})");
            _logger.LogTrace(JsonConvert.SerializeObject(result, Formatting.Indented));
            _logger.LogTrace(receivedString);
        }
    }
}