using System;
using System.Threading.Tasks;
using BundtBot.Discord.Models;
using BundtBot.Discord.Models.Gateway;
using Newtonsoft.Json;

namespace BundtBot.Discord.Gateway
{
    public class GatewayConnectionManager
    {
        internal delegate void SendOpcodeHandler();
        internal event SendOpcodeHandler SendHeartbeat;
        internal event SendOpcodeHandler SendGatewayIdentify;
        internal delegate void SendResumeHandler(string sessionId);
        internal event SendResumeHandler SendResume;

        static readonly MyLogger _logger = new MyLogger(nameof(GatewayConnectionManager), ConsoleColor.Cyan);

        int _numberOfTimesHelloReceived = 0;
        string _sessionId;

        internal void OnHelloReceived(string eventName, string eventData)
        {
            _logger.LogInfo("Received Hello from Gateway", ConsoleColor.Green);
            _numberOfTimesHelloReceived++;
            var hello = JsonConvert.DeserializeObject<GatewayHello>(eventData.ToString());

            // If first time connecting, send identify, else send resume
            if (_numberOfTimesHelloReceived == 1)
            {
                StartHeartBeatLoop(hello.HeartbeatInterval);
                SendGatewayIdentify?.Invoke();
            }
            else if (_numberOfTimesHelloReceived > 1)
            {
                SendResume?.Invoke(_sessionId);
            }
        }

        internal void OnReadyReceived(Ready readyInfo)
        {
            _logger.LogInfo("Received Ready from Gateway", ConsoleColor.Green);
            _sessionId = readyInfo.SessionId;
        }

        void StartHeartBeatLoop(TimeSpan heartbeatInterval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    SendHeartbeat?.Invoke();
                    await Task.Delay(heartbeatInterval);
                }
            });
            _logger.LogInfo($"Heartbeat loop started with interval of {heartbeatInterval.TotalSeconds} seconds", ConsoleColor.Green);
        }
    }
}