using System;
using System.Net.WebSockets;
using BundtBot;
using DiscordApiWrapper.Gateway;

namespace DiscordApiWrapper.WebSocket
{
    partial class WebSocketClient
    {
        static void LogConnected(MyLogger logger, Uri serverUri, ClientWebSocket clientWebSocket)
        {
            logger.LogInfo(
                new LogMessage($"Connected to "),
                new LogMessage($"{serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
        }

        static void LogReconnected(MyLogger logger, Uri serverUri, ClientWebSocket clientWebSocket)
        {
            logger.LogInfo(
                new LogMessage($"Reconnected to "),
                new LogMessage($"{serverUri}", ConsoleColor.Cyan),
                new LogMessage($" (ClientWebSocket State: "),
                new LogMessage($"{clientWebSocket.State}", ConsoleColor.Green),
                new LogMessage($")"));
        }

        static void LogReceiveLoopException(MyLogger logger, Exception ex, ClientWebSocket clientWebSocket)
        {
            logger.LogError("[Receive Loop] Exception caught in ReceiveLoop.");
            logger.LogError(ex);

            logger.LogWarning($"[Receive Loop] _clientWebSocket.State: {clientWebSocket.State.ToString()}");
            logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatus: {clientWebSocket.CloseStatus.ToString()}");
            logger.LogWarning($"[Receive Loop] _clientWebSocket.CloseStatusDescription: {clientWebSocket.CloseStatusDescription}");

            logger.LogWarning("[Receive Loop] Reconnecting.");
        }

        static void LogCloseReceived(MyLogger logger, string codeString)
        {
            string logMessage = "Received a message from Gateway with Close Status, will reconnect: ";

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
                logger.LogCritical(logMessage);
            }
            else
            {
                logger.LogError(logMessage);
            }
        }
    }
}