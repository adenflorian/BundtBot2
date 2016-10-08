namespace BundtBot.Discord.Gateway.Operation
{
	public class HeartbackAckOperation : IGatewayCommand
	{
		public static readonly HeartbackAckOperation Instance = new HeartbackAckOperation();

		readonly MyLogger _logger = new MyLogger(nameof(ClientWebSocketWrapper));

		public void Execute(string eventName, object eventData)
		{
			_logger.LogInfo("HeartbackAck Received ← ♥");
		}
	}
}
