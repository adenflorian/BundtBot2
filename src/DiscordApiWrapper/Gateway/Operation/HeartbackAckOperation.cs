using System;

namespace BundtBot.Discord.Gateway.Operation
{
	public class HeartbackAckOperation : IGatewayCommand
	{
		public static readonly HeartbackAckOperation Instance = new HeartbackAckOperation();

		readonly MyLogger _logger = new MyLogger(nameof(HeartbackAckOperation));

		public void Execute(string eventName, object eventData)
		{
			_logger.LogInfo(
				new LogMessage("HeartbackAck Received ← "),
				new LogMessage("♥", ConsoleColor.Red));
		}
	}
}
